// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.NavigateTo;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Search.Data;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Search
{
    internal sealed class VisualStudioSearchResult : CodeSearchResult
    {
        private readonly INavigateToSearchResult _navigateToSearchResult;
        private readonly INavigableItem _navigableItem;
        private readonly IThreadingContext _threadingContext;
        private readonly IUIThreadOperationExecutor _threadOperationExecutor;

        private VisualStudioSearchResult(
            INavigateToSearchResult navigateToSearchResult,
            INavigableItem navigableItem,
            IThreadingContext threadingContext,
            IUIThreadOperationExecutor threadOperationExecutor,
            string resultId,
            string resultType,
            string primarySortText,
            string secondarySortText,
            string location,
            SearchResultFlags flags)
            : base(
                  resultId,
                  resultType,
                  primarySortText,
                  secondarySortText,
                  location,
                  tieBreakingSortText: null,
                  perProviderItemPriority: 0,
                  view: null,
                  flags,
                  language: null,
                  properties: null)
        {
            _navigateToSearchResult = navigateToSearchResult;
            _navigableItem = navigableItem;
            _threadingContext = threadingContext;
            _threadOperationExecutor = threadOperationExecutor;
            this.Location = base.Location;
        }

        internal static VisualStudioSearchResult Create(
            INavigateToSearchResult navigateToSearchResult,
            INavigableItem navigableItem,
            IThreadingContext threadingContext,
            IUIThreadOperationExecutor threadOperationExecutor)
        {
            var resultId
                = CodeSearchResult.CreateResultId(
                    navigateToSearchResult.Name,
                    navigableItem.Document.FilePath ?? string.Empty,
                    navigableItem.SourceSpan.Start,
                    navigableItem.SourceSpan.End);

            var resultType = MapToCodeSearchResultType(navigateToSearchResult);

            var primarySortText = navigateToSearchResult.Name;
            var secondarySortText = navigateToSearchResult.AdditionalInformation;
            var location = navigableItem.Document.FilePath ?? string.Empty;
            var flags = SearchResultFlags.PrimaryIsPatternSearch | SearchResultFlags.SecondaryIsPatternSearch;

            return new VisualStudioSearchResult(
                navigateToSearchResult,
                navigableItem,
                threadingContext,
                threadOperationExecutor,
                resultId,
                resultType,
                primarySortText,
                secondarySortText,
                location,
                flags);
        }

        public new string? Location { get; set; }

        public async Task PopulateViewAsync(Solution solution)
        {
            if (this.View == null)
            {
                var solutionDirectory = Directory.GetParent(solution.FilePath).FullName;
                if (!string.IsNullOrEmpty(solutionDirectory) && !string.IsNullOrEmpty(this.Location) && Path.IsPathRooted(this.Location))
                {
                    this.Location = MakeRelativePath(solutionDirectory!, this.Location!);
                }

                this.View = new VisualStudioSearchResultView(_navigateToSearchResult, _navigableItem, _threadingContext, _threadOperationExecutor);
            }
        }

        private static string MapToCodeSearchResultType(INavigateToSearchResult navigateToSearchResult)
        {
            return navigateToSearchResult.Kind switch
            {
                NavigateToItemKind.Event => CodeSearchResultType.Event,
                NavigateToItemKind.File => CodeSearchResultType.File,
                NavigateToItemKind.Module => CodeSearchResultType.Module,
                NavigateToItemKind.Class => CodeSearchResultType.Class,
                NavigateToItemKind.Method => CodeSearchResultType.Method,
                NavigateToItemKind.Property => CodeSearchResultType.Property,
                NavigateToItemKind.Field => CodeSearchResultType.Field,
                NavigateToItemKind.Enum => CodeSearchResultType.Enum,
                NavigateToItemKind.Interface => CodeSearchResultType.Interface,
                NavigateToItemKind.Constant => CodeSearchResultType.Constant,
                _ => CodeSearchResultType.OtherSymbol,
            };
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// Note: in .Net Core 2.0 and higher, we can use https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getrelativepath?view=net-6.0.
        /// </summary>
        /// <param name="relativeTo">Contains the directory that defines the start of the relative path.</param>
        /// <param name="path">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        private static string MakeRelativePath(string relativeTo, string path)
        {
            Requires.NotNullOrEmpty(relativeTo, nameof(relativeTo));
            Requires.NotNullOrEmpty(path, nameof(path));

            if (!Uri.TryCreate(relativeTo, UriKind.RelativeOrAbsolute, out Uri relativeToUri)
                || !Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri pathUri))
            {
                return path;
            }

            if (relativeToUri.Scheme != pathUri.Scheme)
            {
                // path can't be made relative.
                return path;
            }

            Uri relativeUri = relativeToUri.MakeRelativeUri(pathUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (pathUri.IsFile)
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
