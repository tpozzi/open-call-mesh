using OpenCallMesh.Core;
using OpenCallMesh.Domain;

var guard = new LoopGuard();
var frame = new AudioFrame("s", "a", "e", 1, 1, [1]);
if (!guard.ShouldForward(frame, "b", "f") || guard.ShouldForward(frame, "a", "f")) throw new Exception("LoopGuard failed");
var jitter = new JitterBuffer();
jitter.Add(frame with { Sequence = 1 }); jitter.Add(frame with { Sequence = 0 });
if (!jitter.TryRead(out var read) || read?.Sequence != 0) throw new Exception("JitterBuffer failed");
var source = new FakeAudioSource([frame with { Sequence = 2 }, frame with { Sequence = 3 }]);
var sink = new FakeAudioSink();
if (await SyntheticRoute.CopyAsync(source, sink, guard, "b", "f") != 2 || sink.Frames.Count != 2) throw new Exception("Synthetic A->B failed");
var controller = new OpenCallMesh.Controller.ControllerRuntime();
controller.Register(new AgentIdentity("agent-a", "machine", "instance", "0.1.0", new HashSet<string> { "pcm" }));
if (controller.Agents.Count != 1) throw new Exception("Controller registration failed");
var converted = CanonicalAudioConverter.StereoFloat32ToMono48K([1, -1, 0.5f, 0.5f, 0, 0], 48000);
var resampled = CanonicalAudioConverter.StereoFloat32ToMono48K(new float[441 * 2], 44100);
if (converted.Length != 3 || Math.Abs(converted[0]) > 0.0001f || Math.Abs(converted[1] - 0.5f) > 0.0001f || resampled.Length != 480) throw new Exception("Canonical audio conversion failed");
Console.WriteLine("Core tests passed");
