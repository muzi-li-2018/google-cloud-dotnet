﻿// Copyright 2017 Google Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Api;
using Google.Api.Gax;
using Google.Cloud.ErrorReporting.V1Beta1;
using Google.Cloud.Logging.V2;

namespace Google.Cloud.Diagnostics.Common
{
    /// <summary>
    /// The location error events will be sent to.
    /// </summary>
    public enum EventTargetKind
    {
        /// <summary>Stackdriver Error Reporting API.</summary>
        ErrorReporting,

        /// <summary>Stackdriver Error Logging API.</summary>
        Logging
    }

    /// <summary>
    /// Represents the location error events will be sent, such as the Stackdriver Logging or Error Reporting API.
    /// </summary>
    public sealed class EventTarget
    {
        /// <summary>The default log name, this is the log that error events will be written to.</summary>
        internal const string LogNameDefault = "stackdriver-error-reporting";

        /// <summary>The global resource.</summary>
        internal static readonly MonitoredResource GlobalResource = new MonitoredResource { Type = "global" };

        /// <summary>The location to send error events to.</summary>
        public EventTargetKind Kind { get; private set; }

        /// <summary>The error reporting client.</summary>
        public ReportErrorsServiceClient ErrorReportingClient { get; private set; }

        /// <summary>The logging client.</summary>
        public LoggingServiceV2Client LoggingClient { get; private set; }

        /// <summary>Where to log to, such as project or organization.</summary>
        public LogTarget LogTarget { get; private set; }

        /// <summary>The name of the log.</summary>
        public string LogName { get; private set; }

        /// <summary>The resource being monitored.</summary>
        public MonitoredResource MonitoredResource { get; private set; }

        /// <summary>
        /// Creates a new <see cref="EventTarget"/> instance that will report to the Stackdriver Logging API.
        /// The events are then automatically propigated to the Stackdriver Error Logging API from the 
        /// Stackdriver Logging API.
        /// </summary>
        /// <remarks>
        /// For more information see "Formatting Log Error Messages"
        /// (https://cloud.google.com/error-reporting/docs/formatting-error-messages).
        /// </remarks>
        /// <param name="projectId">The Google Cloud Platform project Id. Cannot be null.</param>
        /// <param name="logName">The log name.  Cannot be null.</param>
        /// <param name="loggingClient">The logging client.</param>
        /// <param name="monitoredResource">The resource to monitor.</param>
        public static EventTarget ForLogging(string projectId, string logName = LogNameDefault,
            LoggingServiceV2Client loggingClient = null, MonitoredResource monitoredResource = null)
        {
            var logTarget = LogTarget.ForProject(GaxPreconditions.CheckNotNull(projectId, nameof(projectId)));
            return ForLogging(logTarget, logName, loggingClient, monitoredResource);
        }

        /// <summary>
        /// Creates a new <see cref="EventTarget"/> instance that will report to the Stackdriver Logging API.
        /// The events are then automatically propigated to the Stackdriver Error Logging API from the 
        /// Stackdriver Logging API.
        /// </summary>
        /// <remarks>
        /// For more information see "Formatting Log Error Messages"
        /// (https://cloud.google.com/error-reporting/docs/formatting-error-messages).
        /// </remarks>
        /// <param name="logTarget">Where to log to, such as a project or organization. Cannot be null.</param>
        /// <param name="logName">The log name.  Cannot be null.</param>
        /// <param name="loggingClient">The logging client.</param>
        /// <param name="monitoredResource">The resource to monitor.</param>
        public static EventTarget ForLogging(LogTarget logTarget, string logName = LogNameDefault,
            LoggingServiceV2Client loggingClient = null, MonitoredResource monitoredResource = null)
        {
            return new EventTarget
            {
                Kind = EventTargetKind.Logging,
                LoggingClient = loggingClient ?? LoggingServiceV2Client.Create(),
                LogTarget = GaxPreconditions.CheckNotNull(logTarget, nameof(logTarget)),
                LogName = GaxPreconditions.CheckNotNullOrEmpty(logName, nameof(logName)),
                MonitoredResource = monitoredResource ?? GlobalResource,
            };
        }

        /// <summary>
        /// Creates a new <see cref="EventTarget"/> instance that will report to the Stackdriver Error Reporting API.
        /// To use this option you must enable the Stackdriver Error Reporting API
        /// (https://console.cloud.google.com/apis/api/clouderrorreporting.googleapis.com/overview).
        /// </summary>
        /// <param name="errorReportingClient">The error reporting client.</param>
        public static EventTarget ForErrorReporting(ReportErrorsServiceClient errorReportingClient = null)
        {
            return new EventTarget
            {
                Kind = EventTargetKind.ErrorReporting,
                ErrorReportingClient = errorReportingClient ?? ReportErrorsServiceClient.Create(),
            };
        }
    }
}