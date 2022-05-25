﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Highlighting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.KeywordHighlighting.KeywordHighlighters
{
    [ExportHighlighter(LanguageNames.CSharp), Shared]
    internal class CheckedStatementHighlighter : AbstractKeywordHighlighter<CheckedStatementSyntax>
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CheckedStatementHighlighter()
        {
        }

        protected override void AddHighlights(CheckedStatementSyntax checkedStatement, List<TextSpan> highlights, CancellationToken cancellationToken)
            => highlights.Add(checkedStatement.Keyword.Span);
    }
}
