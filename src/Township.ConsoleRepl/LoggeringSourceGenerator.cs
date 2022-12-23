    //using System;
    //using System.Collections.Immutable;
    //using Microsoft.CodeAnalysis;
    //using Microsoft.CodeAnalysis.CSharp;
    //using Microsoft.CodeAnalysis.CSharp.Syntax;

    //[Generator]
    //public class LoggingGenerator : IIncrementalSourceGenerator
    //{
    //    public void Initialize(GeneratorInitializationContext context)
    //    {
    //        // Perform any initialization logic here
    //    }

    //    public void Execute(GeneratorExecutionContext context, SyntaxReceiver receiver)
    //    {
    //        // Get the log level and log message format from the generator arguments
    //        var logLevel = GetLogLevel(context.Compilation.Options.SpecificDiagnosticOptions);
    //        var logMessageFormat = GetLogMessageFormat(context.Compilation.Options.SpecificDiagnosticOptions);

    //        // Go through every changed syntax node
    //        receiver.OnVisitSyntaxNode += (syntaxNode) =>
    //        {
    //            // Check if the syntax node is a method declaration
    //            if (syntaxNode is MethodDeclarationSyntax methodDeclaration)
    //            {
    //                // Generate the logging code for the method
    //                var logEntering = GenerateLogStatement(logLevel, logMessageFormat, "Entering {0}");
    //                var logExiting = GenerateLogStatement(logLevel, logMessageFormat, "Exiting {0}");
    //                var tryStatement = SyntaxFactory.TryStatement(methodDeclaration.Body, default, default, SyntaxFactory.List(new[] { logExiting }));
    //                var newMethodDeclaration = methodDeclaration.WithBody(SyntaxFactory.Block(logEntering, tryStatement));

    //                // Replace the original method declaration with the modified one
    //                context.ReportModifiedSyntaxNode(methodDeclaration, newMethodDeclaration);
    //            }
    //        };
    //    }

    //    private static StatementSyntax GenerateLogStatement(string logLevel, string logMessageFormat, string logMessage)
    //    {
    //        // Generate the log statement using the specified log level, log message format, and log message
    //        return SyntaxFactory.ExpressionStatement(
    //            SyntaxFactory.InvocationExpression(
    //                SyntaxFactory.MemberAccessExpression(
    //                    SyntaxKind.SimpleMemberAccessExpression,
    //                    SyntaxFactory.IdentifierName("Console"),
    //                    SyntaxFactory.IdentifierName("WriteLine")))
    //            .WithArgumentList(
    //                SyntaxFactory.ArgumentList(
    //                    SyntaxFactory.SeparatedList(new[]
    //                    {
    //                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(logLevel))),
    //                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(logMessageFormat)))
    //                    }))));
    //    }

    //    private static string GetLogLevel(ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions)
    //    {
    //        // Get the log level from the generator arguments, or use the default log level if not specified
    //        return specificDiagnosticOptions.TryGetValue("LogLevel", out var logLevel) ? logLevel.ToString() : "Info";
    //    }

    //    private static string GetLogMessageFormat(ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions)
    //    {
    //        // Get the log message format from the generator arguments, or use the default log message format if not specified
    //        return specificDiagnosticOptions.TryGetValue("LogMessageFormat", out var logMessageFormat) ? logMessageFormat.ToString() : "{0}: {1}";
    //    }
    //}

// move to a library maybe