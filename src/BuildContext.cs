using Orel.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Orel
{
    internal class BuildContext
    {
        public ParameterExpression RootObject { get; set; }
        public ParameterExpression CurrentObject { get; set; }
        public BuildContext ParentContext { get; set; }
        public IMemberDescriptor GlobalMemberDescriptor { get; set; }
        public string MemberPrefix { get; set; }
        public ParameterExpression IndexExpression { get; set; }
        internal IMemberDescriptor LocalMemberDescriptor { get; private set; }
        internal string TempPrefix { get; set; }
        internal ParameterManager ParameterManager { get; set; }

        internal static BuildContext Create(ParameterExpression parameter, IMemberDescriptor memberDescriptor, IEnumerable<ParameterDefinition> parameters = null)
        {
            var context = new BuildContext(parameter, memberDescriptor);
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    context.ParameterManager.AddPreDefined(p);
                }
            }
            return context;
        }

        internal static BuildContext Create(IEnumerable<ParameterDefinition> parameters = null)
        {
            var context = new BuildContext() { ParameterManager = new ParameterManager() };
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    context.ParameterManager.AddPreDefined(p);
                }
            }
            return context;
        }

        private BuildContext(ParameterExpression parameter, IMemberDescriptor memberDescriptor)
        {
            CurrentObject = parameter;
            RootObject = parameter;
            GlobalMemberDescriptor = memberDescriptor;
            ParameterManager = new ParameterManager();
            ExternalMethods = new Dictionary<string, IList<Delegate>>(StringComparer.OrdinalIgnoreCase);
        }

        private BuildContext()
        {
            ExternalMethods = new Dictionary<string, IList<Delegate>>(StringComparer.OrdinalIgnoreCase);
        }

        internal BuildContext GenerateChildContext(Type paramType, string parentPath, IMemberDescriptor propagatingMembers = null)
        {
            if (paramType == null)
            {
                paramType = propagatingMembers?.RootType == RootType.List ? typeof(IList) : typeof(object);
            }
            return GenerateChildContext(Expression.Parameter(paramType, "param"), parentPath, propagatingMembers);
        }

        internal BuildContext GenerateChildContext(ParameterExpression paramObject, string parentPath, IMemberDescriptor propagatingMembers = null)
        {
            return new BuildContext()
            {
                RootObject = RootObject,
                ParentContext = this,
                CurrentObject = paramObject,
                MemberPrefix = string.IsNullOrEmpty(MemberPrefix) ? parentPath : $"{MemberPrefix}.{parentPath}",
                GlobalMemberDescriptor = GlobalMemberDescriptor,
                ParameterManager = ParameterManager,
                LocalMemberDescriptor = propagatingMembers == null ? new InternalMemberDescriptor() : propagatingMembers
            };
        }

        internal IMemberDefinition FindMemberDefinition(string memberName)
        {
            return FindMemberDefinition(memberName, MemberPrefix);
        }

        internal IMemberDefinition FindListItemDefinition(IMemberDefinition parentMember)
        {
            if (parentMember == null)
                return FindMemberDefinition(TokenDefinitions.DefaultArgument);
            var defaultMemberName = $"{parentMember.UniqueName}._";
            return FindMemberDefinition(defaultMemberName);
        }

        internal (IMemberDefinition member, BuildContext context) LookupMemberDefinition(string memberName)
        {
            var member = FindMemberDefinition(memberName, MemberPrefix);
            if (member == null && ParentContext != null)
            {
                return ParentContext.LookupMemberDefinition(memberName);
            }
            return (member, this);
        }

        internal IMemberDefinition FindMemberDefinition(string memberName, string prefix)
        {
            IMemberDefinition result;
            if (LocalMemberDescriptor != null)
            {
                result = LocalMemberDescriptor.Get(memberName, null);
                if (result != null) return result;
            }
            if (GlobalMemberDescriptor != null)
            {
                result = GlobalMemberDescriptor.Get(memberName, prefix);
                if (result != null) return result;
            }
            return null;
        }

        internal Dictionary<string, IList<Delegate>> ExternalMethods { get; set; }
    }
}
