﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Microsoft.Azure.WebJobs.Extensions.Files.Bindings
{
    internal class FileTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly FilesConfiguration _config;
        private readonly TraceWriter _trace;

        public FileTriggerAttributeBindingProvider(FilesConfiguration config, TraceWriter trace)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (trace == null)
            {
                throw new ArgumentNullException("trace");
            }

            _config = config;
            _trace = trace;
        }

        /// <inheritdoc/>
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ParameterInfo parameter = context.Parameter;
            FileTriggerAttribute attribute = parameter.GetCustomAttribute<FileTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            // next, verify that the type is one of the types we support
            IEnumerable<Type> types = StreamValueBinder.GetSupportedTypes(FileAccess.Read)
                .Union(new Type[] { typeof(FileStream), typeof(FileSystemEventArgs), typeof(FileInfo) });
            if (!ValueBinder.MatchParameterType(context.Parameter, types))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, 
                    "Can't bind FileTriggerAttribute to type '{0}'.", parameter.ParameterType));
            }

            return Task.FromResult<ITriggerBinding>(new FileTriggerBinding(_config, parameter, _trace));
        }
    }
}
