using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime 
{
	[PublicAPI]
	public interface IApplicationHost 
	{
		[NotNull]
		Application Application { get; }
		
		void Execute([NotNull] Action action, TaskPriority priority = TaskPriority.Normal);
		T Execute<T>([NotNull] Func<T> func, TaskPriority priority = TaskPriority.Normal);
		Task ExecuteAsync([NotNull] Action action, TaskPriority priority = TaskPriority.Normal);
		Task<T> ExecuteAsync<T>([NotNull] Func<T> func, TaskPriority priority = TaskPriority.Normal);

		[ContractAnnotation("=> halt")]
		void Shutdown(int exitCode = 0);
	}
}