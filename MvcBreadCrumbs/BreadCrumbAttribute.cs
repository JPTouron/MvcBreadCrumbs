using System;
using System.Web.Mvc;

namespace MvcBreadCrumbs
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class BreadCrumbAttribute : ActionFilterAttribute
    {
        public bool Clear { get; set; }

        public string Label { get; set; }

        public Type ResourceType { get; set; }

        private static IProvideBreadCrumbsSession _SessionProvider { get; set; }

        private static IProvideBreadCrumbsSession SessionProvider
        {
            get
            {
                if (_SessionProvider != null)
                {
                    return _SessionProvider;
                }
                return new HttpSessionProvider();
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                //if we have an exception on the result (for ANY reason), let's make sure that we don't
                //track this failing page on the breadcrumb
                var state = StateManager.GetState(SessionProvider.SessionId);
                state.OnErrorRemoveCrumb(filterContext);
            }

            base.OnResultExecuted(filterContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            if (filterContext.HttpContext.Request.HttpMethod != "GET")
                return;

            if (Clear)
            {
                StateManager.RemoveState(SessionProvider.SessionId);
            }

            var state = StateManager.GetState(SessionProvider.SessionId);
            state.Push(filterContext, Label, ResourceType);
        }
    }
}