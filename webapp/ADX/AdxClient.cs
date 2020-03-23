using BcdrTestAppADX.Models;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace BcdrTestAppADX.ADX
{
    public class AdxClient
    {
        public ICslQueryProvider adx;

        public KustoConnectionStringBuilder _connection;

        private String _clientRequestIdPrefix;

        private Timer _timer;

        private bool _isUsable = true;

        public AdxClient(String adxUrl, String clientRequestIdPrefix = "BCDR", IAuthenticationSetting authentication = null)
        {
            _clientRequestIdPrefix = clientRequestIdPrefix;

            if(String.IsNullOrEmpty(authentication.ManagedIdentity))
            {
                _connection = new KustoConnectionStringBuilder(adxUrl)
                    .WithAadApplicationKeyAuthentication(authentication.ServicePrincipal.ClientId, authentication.ServicePrincipal.ClientSecret, authentication.ServicePrincipal.TenantId);
            }
            else
            {
                _connection = new KustoConnectionStringBuilder(adxUrl).WithAadManagedIdentity(authentication.ManagedIdentity);
            }

            adx = KustoClientFactory.CreateCslQueryProvider(_connection);

            _timer = new System.Threading.Timer(
                e => CheckUsability(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(2));

        }

        private void CheckUsability()
        {
            if (!_isUsable)
            {
                try
                {
                    var resultTask = Task.Run(() => adx.ExecuteQuery(".show databases | count"));

                    if (resultTask.Wait(30000))
                    {
                        _isUsable = true;
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        public IDataReader ExecuteQuery(String dbName, string query, TelemetryClient telemetry)
        {
            if (_isUsable)
            {
                ClientRequestProperties clientRequestProperties = CreateRequestProperties();

                try
                {
                    var resultTask = Task.Run(() => adx.ExecuteQuery(dbName, query, clientRequestProperties));

                    if (resultTask.Wait(30000))
                    {
                        return resultTask.Result;
                    }
                }
                catch (Exception ex)
                {
                    telemetry.TrackException(ex);
                }

                _isUsable = false;
            }

            return null;
        }

        private ClientRequestProperties CreateRequestProperties()
        {
            var queryParameters = new Dictionary<String, String>()
            {
                //{ "xIntValue", "111" },
                // { "xStrValue", "abc" },
                // { "xDoubleValue", "11.1" }
            };

            var clientRequestProperties = new Kusto.Data.Common.ClientRequestProperties(
                options: null,
                parameters: queryParameters);

            clientRequestProperties.ClientRequestId = _clientRequestIdPrefix + Guid.NewGuid().ToString();
            return clientRequestProperties;
        }
    }
}
