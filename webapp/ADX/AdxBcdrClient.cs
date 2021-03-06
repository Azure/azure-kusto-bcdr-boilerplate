﻿using BcdrTestAppADX.Models;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BcdrTestAppADX.ADX
{
    public class AdxBcdrClient
    {
        private AdxClient _primary;
        private List<AdxClient> _secondaries;

        private readonly Dictionary<string, string> _primaryProperties = new Dictionary<string, string>() { { "adx", "primary" } };
        private readonly Dictionary<string, string> _secondaryProperties = new Dictionary<string, string>() { { "adx", "secondary" } };

        public AdxBcdrClient(IBcdrSetting setting)
        {
            Configure(setting);
        }

        private void Configure(IBcdrSetting setting)
        {
            _primary = new AdxClient(setting.PrimaryAdx, "BCDR-primary-", setting.Authentication);

            _secondaries = new List<AdxClient>();

            var secondaryAdx = setting.SecondaryAdx.Split(';').Select(_ => _.Trim());

            foreach (var aSecondaryUrl in secondaryAdx)
            {
                _secondaries.Add(new AdxClient(aSecondaryUrl, "BCDR-secondary-", setting.Authentication));
            }
        }

        public IDataReader ExecuteQuery(String dbName, string query, TelemetryClient telemetry)
        {
            var result = ExecuteQuery_internal(_primary, dbName, query, telemetry, _primaryProperties);

            if (result != null && !result.IsFaulted)
            {
                return result.Result;
            }
            else
            {
                return ExecuteQuery_internal_secondaries(dbName, query, telemetry);
            }
        }

        private async Task<IDataReader> ExecuteQuery_internal(AdxClient aADXClient, string dbName, string query, TelemetryClient telemetry, Dictionary<string, string> telemetryProperties)
        {
            var result = await aADXClient.ExecuteQueryAsync(dbName, query, telemetry);

            if (result != null)
            {
                telemetry.TrackEvent("adxSuccessfulQuery", telemetryProperties);
            }
            else
            {
                telemetry.TrackEvent("adxFailedQuery", telemetryProperties);
            }

            return result;
        }


        public async Task<IDataReader> ExecuteQueryAsync(String dbName, string query, TelemetryClient telemetry)
        {
            var result = await ExecuteQuery_internal(_primary, dbName, query, telemetry, _primaryProperties);

            if (result != null)
            {
                return result;
            }
            else
            {
                return ExecuteQuery_internal_secondaries(dbName, query, telemetry);
            }
        }

        private IDataReader ExecuteQuery_internal_secondaries(string dbName, string query, TelemetryClient telemetry)
        {
            List<Task<IDataReader>> tasks = new List<Task<IDataReader>>();

            foreach (var aADXClient in _secondaries)
            {
                tasks.Add(Task.Run(() => ExecuteQuery_internal(aADXClient, dbName, query, telemetry, _secondaryProperties)));
            }

            return Task.WhenAll(tasks).Result.Where(_ => _ != null).FirstOrDefault();
        }
    }
}
