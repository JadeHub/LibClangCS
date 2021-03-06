﻿using System;
using System.Collections.Generic;

namespace LibClang
{
    /// <summary>
    /// An immutable wrapper around libclang's Diagnostic type.
    /// A Diagnostic represents a diagnostic message generated during parsing.
    /// 
    /// </summary>
    public sealed class Diagnostic : IDisposable
    {
        #region Enums

        public enum Severity
        {
            /// <summary>
            /// A diagnostic that has been suppressed, e.g., by a command-line option.
            /// </summary>
            Ignored = 0,

            /// <summary>
            /// This diagnostic is a note that should be attached to the previous (non-note) diagnostic.
            /// </summary>
            Note = 1,

            /// <summary>
            /// This diagnostic indicates suspicious code that may not be wrong.
            /// </summary>
            Warning = 2,

            /// <summary>
            /// This diagnostic indicates that the code is ill-formed.
            /// </summary>
            Error = 3,

            /// <summary>
            /// This diagnostic indicates that the code is ill-formed such that future parser recovery is unlikely to produce useful results.
            /// </summary>
            Fatal = 4
        }

        enum CXDiagnosticDisplayOptions
        {
            /// <summary>
            /// Display the source-location information where the diagnostic was located.
            ///
            /// When set, diagnostics will be prefixed by the file, line, and
            /// (optionally) column to which the diagnostic refers. For example,
            ///
            /// test.c:28: warning: extra tokens at end of #endif directive
            /// 
            ///  This option corresponds to the clang flag \c -fshow-source-location.        
            /// </summary>
            CXDiagnostic_DisplaySourceLocation = 0x01,

            /// <summary>
            /// If displaying the source-location information of the
            /// diagnostic, also include the column number.
            ///
            /// This option corresponds to the clang flag \c -fshow-column.
            /// </summary>
            CXDiagnostic_DisplayColumn = 0x02,

            /// <summary>
            /// If displaying the source-location information of the
            /// diagnostic, also include information about source ranges in a
            /// machine-parsable format.
            ///
            /// This option corresponds to the clang flag
            /// -fdiagnostics-print-source-range-info.
            /// </summary>
            CXDiagnostic_DisplaySourceRanges = 0x04,

            /// <summary>
            /// Display the option name associated with this diagnostic, if any.
            ///
            /// The option name displayed (e.g., -Wconversion) will be placed in brackets
            /// after the diagnostic text. This option corresponds to the clang flag
            /// -fdiagnostics-show-option.
            /// </summary>
            CXDiagnostic_DisplayOption = 0x08,

            /// <summary>
            /// Display the category number associated with this diagnostic, if any.
            ///
            /// The category number is displayed within brackets after the diagnostic text.
            /// This option corresponds to the clang flag 
            /// -fdiagnostics-show-category=id.
            /// </summary>
            CXDiagnostic_DisplayCategoryId = 0x10,

            /// <summary>
            /// Display the category name associated with this diagnostic, if any.
            ///
            /// The category name is displayed within brackets after the diagnostic text.
            /// This option corresponds to the clang flag 
            /// -fdiagnostics-show-category=name.
            /// </summary>
            CXDiagnostic_DisplayCategoryName = 0x20
        };

        #endregion

        #region Data

        private readonly ITranslationUnitItemFactory _itemFactory;

        internal IntPtr Handle { get; private set; }

        private string _spelling;
        private SourceLocation _location;
        private Tuple<string, string> _option;
        private readonly IList<SourceRange> _ranges;
        private Tuple<string, SourceRange>[] _fixIts;

        #endregion

        #region Lifetime

        /// <summary>
        /// Create a new Diagnostic object.
        /// </summary>
        /// <param name="handle">Handle to a diagnostic object.</param>
        /// <param name="itemFactory">TranslationUnit's item factory / item cache.</param>
        internal Diagnostic(IntPtr handle, ITranslationUnitItemFactory itemFactory)
        {
            _itemFactory = itemFactory;
            Handle = handle;
            _ranges = new List<SourceRange>();
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                Library.clang_disposeDiagnostic(Handle);
                Handle = IntPtr.Zero;
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Format the diagnostic using the suplied display options.
        /// </summary>
        /// <param name="formatOptions">Combination of CXDiagnosticDisplayOptions values</param>
        /// <returns>Formatted diagnostic.</returns>
        public string Format(uint formatOptions)
        {
            return Library.clang_formatDiagnostic(Handle, formatOptions).ManagedString;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Format the diagnostic using Clang's default display options.
        /// </summary>
        public string DefaultFormat
        {
            get { return Library.clang_formatDiagnostic(Handle, Library.clang_defaultDiagnosticDisplayOptions()).ManagedString; }
        }

        /// <summary>
        /// Return the Severity of this Diagnostic.
        /// </summary>
        public Severity DiagnosticSeverity
        {
            get { return Library.clang_getDiagnosticSeverity(Handle); }
        }

        /// <summary>
        /// Return the Spelling of this Diagnostic.
        /// </summary>
        public string Spelling
        {
            get { return _spelling ?? (_spelling = Library.clang_getDiagnosticSpelling(Handle).ManagedString);}
        }

        /// <summary>
        /// Return the Location of this Diagnostic.
        /// </summary>
        public SourceLocation Location
        {
            get { return _location ?? (_location = _itemFactory.CreateSourceLocation(Library.clang_getDiagnosticLocation(Handle))); }
        }

        private unsafe Tuple<string, string> LoadOption()
        {
            Library.ClangString enable, disable;
            enable = Library.clang_getDiagnosticOption(Handle, &disable);
            return new Tuple<string, string>(enable.ManagedString, disable.ManagedString);
        }

        /// <summary>
        /// Return a Tuple containing the compiler options used to enable and disable this Diagnostic.
        /// </summary>
        public Tuple<string, string> Option
        {
            get { return _option ?? (_option = LoadOption()); }
        }

        /// <summary>
        /// Return the Category of this Diagnostic.
        /// </summary>
        public uint Category
        {
            get { return Library.clang_getDiagnosticCategory(Handle); }
        }

        /// <summary>
        /// Return the CategoryName of this Diagnostic.
        /// </summary>
        public string CategoryName
        {
            get { return Library.clang_getDiagnosticCategoryName(Category).ManagedString; }
        }

        /// <summary>
        /// Return the CategoryText of this Diagnostic.
        /// </summary>
        public string CategoryText
        {
            get { return Library.clang_getDiagnosticCategoryText(Handle).ManagedString; }
        }

        /// <summary>
        /// Return the SourceRangles associated with this Diagnostic.
        /// </summary>
        public IEnumerable<SourceRange> Ranges
        {
            get
            {
                uint count = Library.clang_getDiagnosticNumRanges(Handle);
                for (uint i = 0; i < count; i++)
                {
                    yield return _itemFactory.CreateSourceRange(Library.clang_getDiagnosticRange(Handle, i));
                }
            }
        }

        private unsafe void LoadFixIts()
        {
            uint count = Library.clang_getDiagnosticNumFixIts(Handle);
            if (count > 0)
            {
                _fixIts = new Tuple<string, SourceRange>[count];
                for (uint i = 0; i < count; i++)
                {
                    Library.SourceRange rangeHandle;
                    string fixIt = Library.clang_getDiagnosticFixIt(Handle, i, &rangeHandle).ManagedString;
                    _fixIts[i] = new Tuple<string, SourceRange>(fixIt, _itemFactory.CreateSourceRange(rangeHandle));
                }
            }
        }

        /// <summary>
        /// Return an array of Tuples containing text and location for each fix-it.
        /// </summary>
        public Tuple<string, SourceRange>[] FixIts
        {
            get
            {
                if (_fixIts == null)
                {
                    LoadFixIts();
                }
                return _fixIts;
            }
        }
    
        #endregion

        #region object overrides

        public override string ToString()
        {
            return Spelling;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Diagnostic)
                return Handle.Equals(((Diagnostic)obj).Handle);
            return false;
        }

        #endregion

        #region Static operator functions

        public static bool operator ==(Diagnostic left, Diagnostic right)
        {
            if ((object)left == null && (object)right == null)
                return true;
            if ((object)left == null || (object)right == null)
                return false;
            return left.Handle == right.Handle;
        }

        public static bool operator !=(Diagnostic left, Diagnostic right)
        {
            if ((object)left == null && (object)right == null)
                return false;
            if ((object)left == null || (object)right == null)
                return true;
            return left.Handle != right.Handle;
        }
        
        #endregion
    }
}
