using Elasticsearch.Net;
using Elasticsearch.Net.Connection.Configuration;
using Newtonsoft.Json;

namespace Nest
{
	public abstract class BaseRequest<TParameters> : IRequest<TParameters>
		where TParameters : IRequestParameters, new()
	{
		[JsonIgnore]
		protected IRequest<TParameters> Request => this;

		/// <summary>
		/// Allows you to override connection settings on a per call basis
		/// </summary>
		[JsonIgnore]
		IRequestConfiguration IRequest.RequestConfiguration { get; set; }

		/// <summary>
		/// Describes parameters that are supplied on the querystring rather then the body of the request
		/// </summary>
		[JsonIgnore]
		TParameters IRequest<TParameters>.RequestParameters { get; set; } = new TParameters();

		/// <summary>
		/// Creates a PathInfo object from this request that we can use to dispatch into the low level client
		/// </summary>
		internal virtual ElasticsearchPathInfo<TParameters> ToPathInfo(
			IConnectionSettingsValues settings, 
			TParameters queryString
			)
		{
			var pathInfo = new ElasticsearchPathInfo<TParameters>
			{
				RequestParameters = queryString
			};
			//if this request describes request specific connection overrides make sure they are carried 
			//over into the pathInfo object
			var config = ((IRequest) this).RequestConfiguration;
			if (config != null)
			{
				IRequestParameters p = pathInfo.RequestParameters;
				p.RequestConfiguration = config;
			}
			
			//ask subclasses to set the relevant pathInfo parameters
			SetRouteParameters(settings, pathInfo);
			//update the pathInfo, is abstract and forces each subclass to at a minimum set the HttpMethod
			UpdatePathInfo(settings, pathInfo);

			ValidatePathInfo(pathInfo);

			return pathInfo;
		}

		protected virtual void SetRouteParameters(IConnectionSettingsValues settings, ElasticsearchPathInfo<TParameters> pathInfo) { }

		protected virtual void ValidatePathInfo(ElasticsearchPathInfo<TParameters> pathInfo)
		{
		}

		protected abstract void UpdatePathInfo(IConnectionSettingsValues settings, ElasticsearchPathInfo<TParameters> pathInfo);
		
		ElasticsearchPathInfo<TParameters> IPathInfo<TParameters>.ToPathInfo(IConnectionSettingsValues settings)
		{
			return this.ToPathInfo(settings, this.Request.RequestParameters);
		}

	}
}