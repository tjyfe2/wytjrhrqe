// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.NavigateTo;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.VisualStudio.LanguageServices.Search.Preview;
using Microsoft.VisualStudio.Search.Data;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Search
{
    internal sealed class VisualStudioSearchResultView : SearchResultViewBase
    {
        private readonly INavigableItem _navigableItem;
        private readonly IThreadingContext _threadingContext;
        private readonly IUIThreadOperationExecutor _threadOperationExecutor;
        private readonly IReadOnlyList<SearchResultInteractiveViewBase> _interactiveViews;

        public VisualStudioSearchResultView(
            INavigateToSearchResult navigateToSearchResult,
            INavigableItem navigableItem,
            IThreadingContext threadingContext,
            IUIThreadOperationExecutor threadOperationExecutor)
            : base(
                  navigateToSearchResult.Name,
                  description: navigateToSearchResult.AdditionalInformation,
                  primaryIcon: navigableItem.Glyph.GetImageId(),
                  flags: SearchResultViewFlags.ExcludeFromMostRecentlyUsed)
        {
            _navigableItem = navigableItem;
            _threadingContext = threadingContext;
            _threadOperationExecutor = threadOperationExecutor;
            _interactiveViews = this.CreateInteractiveView();
        }

        public override IReadOnlyList<SearchResultInteractiveViewBase> InteractiveViews => this._interactiveViews;

        public override void Invoke(CancellationToken cancellationToken)
        {
            var document = _navigableItem.Document;
            if (document == null)
                return;

            var workspace = document.Project.Solution.Workspace;
            var navigationService = workspace.Services.GetService<IDocumentNavigationService>();

            // Document tabs opened by NavigateTo are carefully created as preview or regular tabs
            // by them; trying to specifically open them in a particular kind of tab here has no
            // effect.
            //
            // In the case of a stale item, don't require that the span be in bounds of the document
            // as it exists right now.
            using var context = _threadOperationExecutor.BeginExecute(
                EditorFeaturesResources.Navigating_to_definition, EditorFeaturesResources.Navigating_to_definition, allowCancellation: true, showProgress: false);
            navigationService.TryNavigateToSpanAsync(
                _threadingContext,
                workspace,
                document.Id,
                _navigableItem.SourceSpan,
                NavigationOptions.Default,
                allowInvalidSpan: _navigableItem.IsStale,
                context.UserCancellationToken)
                .ConfigureAwait(false);
        }

        private IReadOnlyList<SearchResultInteractiveViewBase> CreateInteractiveView()
        {
            return new List<SearchResultInteractiveViewBase>
            {
                new FileContentInteractiveView(this.Title, _navigableItem, this.PrimaryIcon),
            };
        }
    }
}
