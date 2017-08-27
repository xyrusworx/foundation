using JetBrains.Annotations;

namespace XyrusWorx.Runtime 
{
	[PublicAPI]
	public enum TaskPriority
	{
		Disabled = 0,
		SystemIdle = 1,
		ApplicationIdle = 2,
		ContextIdle = 3,
		Background = 4,
		Input = 5,
		Ui = 7,
		Data = 8,
		Normal = 9,
		MessageLoop = 10
	}
}