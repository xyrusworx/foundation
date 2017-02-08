using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace XyrusWorx.Communication.Provider
{
	[PublicAPI]
	public abstract class RestService<TEntity, TId> : WebService 
		where TId: struct 
		where TEntity: class
	{
		protected RestService([NotNull] string route) : base(route)
		{
		}

		protected sealed override WebServiceResult Index()
		{
			if (User == null && ProtectedVerbs.HasFlag(WebServiceVerbs.Index))
			{
				return NotAllowed();
			}

			var result = ListOverride().ToArray();

			return StatusCode(200, result);
		}

		[ServiceExport("{id}", WebServiceVerbs.Get)]
		public virtual WebServiceResult Get(TId id)
		{
			if (User == null && ProtectedVerbs.HasFlag(WebServiceVerbs.Get))
			{
				return NotAllowed();
			}

			var result = GetOverride(id);
			if (result == null)
			{
				return NotFound();
			}

			return StatusCode(200, result);
		}

		[ServiceExport(WebServiceVerbs.Post)]
		public virtual WebServiceResult Post([FromBody] TEntity value)
		{
			if (value == null)
			{
				return BadRequest();
			}

			if (User == null && ProtectedVerbs.HasFlag(WebServiceVerbs.Post))
			{
				return NotAllowed();
			}

			var updatedEntity = UpdateOverride(value);

			return StatusCode(200, updatedEntity);
		}

		[ServiceExport("{id}", WebServiceVerbs.Put)]
		public virtual WebServiceResult Put(TId id, [FromBody] TEntity value)
		{
			if (value == null)
			{
				return BadRequest();
			}

			if (User == null && ProtectedVerbs.HasFlag(WebServiceVerbs.Put))
			{
				return NotAllowed();
			}

			var createdEntity = CreateOverride(id, value);

			return StatusCode(200, createdEntity);
		}

		[ServiceExport("{id}", WebServiceVerbs.Delete)]
		public virtual WebServiceResult Delete(TId id)
		{
			if (User == null && ProtectedVerbs.HasFlag(WebServiceVerbs.Delete))
			{
				return NotAllowed();
			}

			DeleteOverride(id);

			return StatusCode(204);
		}

		protected virtual WebServiceVerbs ProtectedVerbs => WebServiceVerbs.None;

		[NotNull]
		protected abstract IEnumerable<TId> ListOverride();

		[CanBeNull]
		protected abstract TEntity GetOverride(TId id);

		[NotNull]
		protected abstract TEntity CreateOverride(TId id, [NotNull] TEntity entity);

		[NotNull]
		protected abstract TEntity UpdateOverride([NotNull] TEntity entity);

		protected abstract void DeleteOverride(TId id);
	}
}