﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities.OutOfProcess;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;
using ProjectUtils = Microsoft.VisualStudio.IntegrationTest.Utilities.Common.ProjectUtils;

namespace Roslyn.VisualStudio.IntegrationTests.VisualBasic
{
    [Collection(nameof(SharedIntegrationHostFixture))]
    [Trait(Traits.Feature, Traits.Features.CodeActionsGenerateType)]
    public class BasicGenerateTypeDialog : AbstractEditorTest
    {
        protected override string LanguageName => LanguageNames.VisualBasic;

        private GenerateTypeDialog_OutOfProc GenerateTypeDialog => VisualStudio.GenerateTypeDialog;

        public BasicGenerateTypeDialog(VisualStudioInstanceFactory instanceFactory)
            : base(instanceFactory, nameof(BasicGenerateTypeDialog))
        {
        }

        [WpfFact]
        public void BasicToCSharp()
        {
            var csProj = new ProjectUtils.Project("CSProj");
            VisualStudio.SolutionExplorer.AddProject(csProj, WellKnownProjectTemplates.ClassLibrary, LanguageNames.CSharp);

            var project = new ProjectUtils.Project(ProjectName);
            VisualStudio.SolutionExplorer.OpenFile(project, "Class1.vb");

            SetUpEditor(@"
Class C
    Sub Method()
        $$Dim _A As A
    End Sub
End Class
");
            VisualStudio.Editor.Verify.CodeAction("Generate new type...",
                applyFix: true,
                blockUntilComplete: false);

            GenerateTypeDialog.VerifyOpen();
            GenerateTypeDialog.SetAccessibility("Public");
            GenerateTypeDialog.SetKind("Structure");
            GenerateTypeDialog.SetTargetProject(csProj.Name);
            GenerateTypeDialog.SetTargetFileToNewName("GenerateTypeTest.cs");
            GenerateTypeDialog.ClickOK();
            GenerateTypeDialog.VerifyClosed();
            var actualText = VisualStudio.Editor.GetText();
            Assert.Contains(@"Imports CSProj

Class C
    Sub Method()
        Dim _A As A
    End Sub
End Class
", actualText);

            VisualStudio.SolutionExplorer.OpenFile(csProj, "GenerateTypeTest.cs");
            actualText = VisualStudio.Editor.GetText();
            Assert.Contains(@"namespace CSProj
{
    public struct A
    {
    }
}", actualText);
        }

        [WpfFact]
        public void SameProject()
        {
            SetUpEditor(@"
Class C
    Sub Method()
        $$Dim _A As A
    End Sub
End Class
");

            VisualStudio.Editor.Verify.CodeAction("Generate new type...",
                applyFix: true,
                blockUntilComplete: false);
            var project = new ProjectUtils.Project(ProjectName);

            GenerateTypeDialog.VerifyOpen();
            GenerateTypeDialog.SetAccessibility("Public");
            GenerateTypeDialog.SetKind("Structure");
            GenerateTypeDialog.SetTargetFileToNewName("GenerateTypeTest");
            GenerateTypeDialog.ClickOK();
            GenerateTypeDialog.VerifyClosed();

            VisualStudio.SolutionExplorer.OpenFile(project, "GenerateTypeTest.vb");
            var actualText = VisualStudio.Editor.GetText();
            Assert.Contains(@"Public Structure A
End Structure
", actualText);

            VisualStudio.SolutionExplorer.OpenFile(project, "Class1.vb");
            actualText = VisualStudio.Editor.GetText();
            Assert.Contains(@"Class C
    Sub Method()
        Dim _A As A
    End Sub
End Class
", actualText);
        }

        [WpfFact]
        public void CheckFoldersPopulateComboBox()
        {
            var project = new ProjectUtils.Project(ProjectName);
            VisualStudio.SolutionExplorer.AddFile(project, @"folder1\folder2\GenerateTypeTests.vb", open: true);

            SetUpEditor(@"Class C
    Sub Method() 
        $$Dim _A As A
    End Sub
End Class
");
            VisualStudio.Editor.Verify.CodeAction("Generate new type...",
                applyFix: true,
                blockUntilComplete: false);

            GenerateTypeDialog.VerifyOpen();
            GenerateTypeDialog.SetTargetFileToNewName("Other");

            var folders = GenerateTypeDialog.GetNewFileComboBoxItems();

            Assert.Contains(@"\folder1\", folders);
            Assert.Contains(@"\folder1\folder2\", folders);

            GenerateTypeDialog.ClickCancel();
        }
    }
}
