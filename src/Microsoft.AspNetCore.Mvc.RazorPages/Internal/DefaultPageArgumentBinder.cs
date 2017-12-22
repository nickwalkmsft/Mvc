﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageArgumentBinder : PageArgumentBinder
    {
        private readonly ParameterBinder _parameterBinder;

        public DefaultPageArgumentBinder(ParameterBinder binder)
        {
            _parameterBinder = binder;
        }

        protected override async Task<ModelBindingResult> BindAsync(PageContext pageContext, object value, string name, Type type)
        {
            var valueProvider = await GetCompositeValueProvider(pageContext);
            var parameterDescriptor = new ParameterDescriptor
            {
                BindingInfo = null,
                Name = name,
                ParameterType = type,
            };

            return await _parameterBinder.BindModelAsync(pageContext, valueProvider, parameterDescriptor, value);
        }

        private static async Task<CompositeValueProvider> GetCompositeValueProvider(PageContext pageContext)
        {
            var factories = pageContext.ValueProviderFactories;
            var valueProviderFactoryContext = new ValueProviderFactoryContext(pageContext);
            for (var i = 0; i < factories.Count; i++)
            {
                var factory = factories[i];
                await factory.CreateValueProviderAsync(valueProviderFactoryContext);
            }

            var loggerFactory = pageContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            return new CompositeValueProvider(valueProviderFactoryContext.ValueProviders, loggerFactory);
        }
    }
}
