// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.VisualStudio.Search.Data;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Search
{
    [Export(typeof(ISearchItemsSourceProvider)), System.Composition.Shared]
    [Name("Go To Code source provider")]
    [ProducesResultType(CodeSearchResultType.Class)]
    [ProducesResultType(CodeSearchResultType.Constant)]
    [ProducesResultType(CodeSearchResultType.Delegate)]
    [ProducesResultType(CodeSearchResultType.Enum)]
    [ProducesResultType(CodeSearchResultType.EnumItem)]
    [ProducesResultType(CodeSearchResultType.Event)]
    [ProducesResultType(CodeSearchResultType.Field)]
    [ProducesResultType(CodeSearchResultType.Interface)]
    [ProducesResultType(CodeSearchResultType.Method)]
    [ProducesResultType(CodeSearchResultType.Module)]
    [ProducesResultType(CodeSearchResultType.OtherSymbol)]
    [ProducesResultType(CodeSearchResultType.Property)]
    [ProducesResultType(CodeSearchResultType.Structure)]
    internal sealed class VisualStudioSearchItemsSourceProvider : ISearchItemsSourceProvider
    {
        private readonly VisualStudioWorkspace _workspace;
        private readonly IThreadingContext _threadingContext;
        private readonly IUIThreadOperationExecutor _threadOperationExecutor;
        private readonly IAsynchronousOperationListener _asyncListener;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public VisualStudioSearchItemsSourceProvider(
            VisualStudioWorkspace workspace,
            IThreadingContext threadingContext,
            IUIThreadOperationExecutor threadOperationExecutor,
            IAsynchronousOperationListenerProvider listenerProvider)
        {
            _workspace = workspace;
            _threadingContext = threadingContext;
            _threadOperationExecutor = threadOperationExecutor;
            _asyncListener = listenerProvider.GetListener(FeatureAttribute.NavigateTo);
        }

        public ISearchItemsSource CreateItemsSource()
        {
            return new VisualStudioSearchItemsSource(
                _workspace,
                _threadingContext,
                _threadOperationExecutor,
                _asyncListener);
        }
    }
}
