// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4SimpleConfigurator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Log
{
    using System;
    using System.Linq;
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    /// <summary>
    /// A Nlog simple configurator used in CDP4
    /// </summary>
    public static class CDP4SimpleConfigurator
    {
        /// <summary>
        /// The current <see cref="LoggingConfiguration"/>
        /// </summary>
        private static LoggingConfiguration configuration;

        /// <summary>
        /// Add a new target for nlog loggers
        /// </summary>
        /// <param name="targetName">the <see cref="Target"/> name</param>
        /// <param name="target">the <see cref="Target"/></param>
        /// <param name="minLogLevel">the minimum <see cref="LogLevel"/> to log</param>
        public static void AddTarget(string targetName, Target target, LogLevel minLogLevel)
        {
            if (LogManager.Configuration == null)
            {
                LogManager.Configuration = new LoggingConfiguration();
                Reload();
            }

            configuration = LogManager.Configuration;

            if (configuration.AllTargets.SingleOrDefault(t => t.Name == targetName) == null)
            {
                target.Name = targetName;
                configuration.AddTarget(targetName, target);
            }

            Reload();
            ChangeTargetRule(target, minLogLevel);
        }

        /// <summary>
        /// Change the log-rule for a <see cref="Target"/>
        /// </summary>
        /// <param name="target">The <see cref="Target"/></param>
        /// <param name="minLogLevel">The minimum <see cref="LogLevel"/> to log</param>
        /// <remarks>Order of <see cref="LogLevel"/> is from minimum: Trace, Debug, Info, Warn, Error, Fatal</remarks>
        public static void ChangeTargetRule(Target target, LogLevel minLogLevel)
        {
            if (!configuration.AllTargets.Contains(target))
            {
                throw new ArgumentOutOfRangeException("target");
            }

            var rule = configuration.LoggingRules.SingleOrDefault(r => r.Targets.Contains(target));
            if (rule != null)
            {
                configuration.LoggingRules.Remove(rule);
            }

            rule = new LoggingRule("*", minLogLevel, target);
            configuration.LoggingRules.Add(rule);
            
            Reload();
        }

        /// <summary>
        /// Reload the configuration file and reset the existing nlog-loggers
        /// </summary>
        private static void Reload()
        {
            LogManager.Configuration.Reload();
            LogManager.ReconfigExistingLoggers();
        }
    }
}