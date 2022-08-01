// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.LanguageServer;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Text.Shared.Extensions;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Search.Data;
using Microsoft.VisualStudio.Search.UI.InteractiveViewPane.Models;

namespace Microsoft.VisualStudio.LanguageServices.Search.Preview
{
    internal sealed class FileContentInteractiveView : SearchResultInteractiveViewBase
    {
        public FileContentInteractiveView(string title, INavigableItem navigableItem, ImageId icon)
            : base(title, icon)
        {
            this.UserInterface
                = new CodeEditorModel(
                    id: "roslynCodeSearchResultCodeEditor",
                    new Threading.AsyncLazy<TextDocumentLocation>(
                        () => Task.FromResult(
                            new TextDocumentLocation(
                                navigableItem.Document.GetURI(),
                                projectId: navigableItem.Document.Project.Id.Id,
                                navigableItem.SourceSpan.ToSpan()))),
                    isEditable: true);
        }

        public override UIBaseModel UserInterface { get; }
    }
}
