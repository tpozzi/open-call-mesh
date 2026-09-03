using OpenCallMesh.Audio.Windows;

if (args is ["audio", "list"])
{
    Console.WriteLine(new WindowsAudioInventory().Status);
    return;
}
Console.WriteLine("OpenCallMesh CLI\nCommands: audio list");
