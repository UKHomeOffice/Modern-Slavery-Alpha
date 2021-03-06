﻿using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Shared.Classes.Extensions
{
    public static partial class Extensions
    {
        public static string WithQuery(this IUrlHelper helper, string actionName, object routeValues)
        {
            var newRoute = new NameValueCollection();
            foreach (var key in helper.ActionContext.HttpContext.Request.Query.Keys)
                newRoute[key] = helper.ActionContext.HttpContext.Request.Query[key];

            foreach (var item in new RouteValueDictionary(routeValues)) newRoute[item.Key] = item.Value.ToString();

            string querystring = null;
            var keys = new SortedSet<string>(newRoute.AllKeys);
            foreach (var key in keys)
            foreach (var value in newRoute.GetValues(key))
            {
                if (string.IsNullOrWhiteSpace(value)) continue;

                if (!string.IsNullOrWhiteSpace(querystring)) querystring += "&";

                querystring += $"{key}={value}";
            }

            return helper.Action(actionName) + "?" + querystring;
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values,
            string protocol)
            where TDestController : BaseController
        {
            var routeValues = new RouteValueDictionary(values);
            var areaAttr = GetControllerArea<TDestController>();
            if (areaAttr != null) routeValues.Add(areaAttr.RouteKey, areaAttr.RouteValue);

            return helper.Action(action, GetControllerFriendlyName<TDestController>(), routeValues, protocol);
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action, object values)
            where TDestController : BaseController
        {
            return helper.Action<TDestController>(action, new RouteValueDictionary(values), "https");
        }

        public static string Action<TDestController>(this IUrlHelper helper, string action)
            where TDestController : BaseController
        {
            return helper.Action<TDestController>(action, null);
        }
    }
}