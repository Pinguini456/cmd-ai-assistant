using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

if (args.Length < 2 || args[0] != "-p")
{
    throw new Exception("Usage: program -p <prompt>");
}

var prompt = args[1];

if (string.IsNullOrEmpty(prompt))
{
    throw new Exception("Prompt must not be empty");
}

var apiKey = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
var baseUrl = "https://models.inference.ai.azure.com";

if (string.IsNullOrEmpty(apiKey))
{
    throw new Exception("GITHUB_TOKEN is not set");
}

List<ChatMessage> messages = [new UserChatMessage(prompt)];
bool requiresAction = true;

// create read tool
var readTool = ChatTool.CreateFunctionTool(
    functionName: "Read",
    functionDescription: "Read and return the contents of a file",
    functionParameters: BinaryData.FromBytes("""
                                             {
                                               "type": "object",
                                               "properties": {
                                                 "file_path": {
                                                   "type": "string",
                                                   "description": "The path to the file to read"
                                                 }
                                               },
                                               "required": ["file_path"]
                                             }
                                             """u8.ToArray())
    );

var writeTool = ChatTool.CreateFunctionTool(
    functionName: "Write",
    functionDescription: "Write content to a file",
    functionParameters: BinaryData.FromBytes("""
                                             {
                                               "type": "object",
                                               "properties": {
                                                 "file_path": {
                                                   "type": "string",
                                                   "description": "The path of the file to write to"
                                                 },
                                                 "content": {
                                                   "type": "string",
                                                   "description": "The content to write to the file"
                                                 }
                                               },
                                               "required": ["file_path", "content"]
                                             }
                                             """u8.ToArray()));

var bashTool = ChatTool.CreateFunctionTool(
    functionName: "Bash",
    functionDescription: "Execute a shell command",
    functionParameters: BinaryData.FromBytes("""
                                             {
                                               "type": "object",
                                               "required": ["command"],
                                               "properties": {
                                                 "command": {
                                                   "type": "string",
                                                   "description": "The command to execute in windows command prompt"
                                                 }
                                               }
                                             }
                                             """u8.ToArray()));

// Add tools to chat completion options
var chatCompletionOptions = new ChatCompletionOptions();
chatCompletionOptions.Tools.Add(readTool);
chatCompletionOptions.Tools.Add(writeTool);
chatCompletionOptions.Tools.Add(bashTool);


var client = new ChatClient(
    model: "gpt-4o",
    credential: new ApiKeyCredential(apiKey),
    options: new OpenAIClientOptions { Endpoint = new Uri(baseUrl) }
);

// ChatCompletion response = client.CompleteChat(
//     [new UserChatMessage(prompt)],
//     chatCompletionOptions
// );

while (requiresAction)
{
    ChatCompletion response = client.CompleteChat(messages, chatCompletionOptions);

    if (response.FinishReason == ChatFinishReason.ToolCalls)
    {
        messages.Add(new AssistantChatMessage(response));
        
        var toolCall = response.ToolCalls[0];
        
        if (toolCall.FunctionName == "Read")
        {
            var argsJson = toolCall.FunctionArguments;
        
            var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(argsJson);
        
            var filePath = parsed["file_path"];

            try
            {
                var fileContent = File.ReadAllText(filePath);

                messages.Add(new ToolChatMessage(toolCall.Id, fileContent));
            } catch (Exception e)
            {
                messages.Add(new ToolChatMessage(toolCall.Id, $"Error:  {e.Message}"));
            }
        } else if (toolCall.FunctionName == "Write")
        {
            var argsJson = toolCall.FunctionArguments;
        
            var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(argsJson);
            
            var filePath = parsed["file_path"];
            var content = parsed["content"];

            try
            {
                File.WriteAllText(filePath, content);

                messages.Add(new ToolChatMessage(toolCall.Id, "Success:  File written successfully."));
            }
            catch (Exception e)
            {
                messages.Add(new ToolChatMessage(toolCall.Id, $"Error:  {e.Message}"));
            }
        } else if (toolCall.FunctionName == "Bash")
        {
            var argsJson = toolCall.FunctionArguments;
        
            var parsed = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(argsJson);
            
            var command = parsed["command"];

            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                process.WaitForExit();


                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                string finalOutput = output;
                if (!string.IsNullOrEmpty(error))
                {
                    finalOutput += "\n" + error;
                }

                if (string.IsNullOrEmpty(finalOutput))
                {
                    finalOutput = "Success: Command executed with no output.";
                }

                messages.Add(new ToolChatMessage(toolCall.Id, finalOutput));
            }
            catch (Exception e)
            {
                messages.Add(new ToolChatMessage(toolCall.Id, $"Error:  {e.Message}"));
            }
        }
    } else if (response.FinishReason == ChatFinishReason.Stop && response.Content.Count > 0)
    {
        Console.Write(response.Content[0].Text);

        requiresAction = false;
    }
    else
    {
        throw new InvalidOperationException($"Unexpected FinishReason: {response.FinishReason}");
    }
}



