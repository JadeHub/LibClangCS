﻿using System;
using System.Diagnostics;

namespace LibClang.Indexer
{
    /// <summary>
    /// Immutable wrapper around libClang's CXIdxDeclInfo type.
    /// Generated by the indexer to represent a declaration found in the translation unit.
    /// </summary>
    public class DeclInfo
    {
        #region Data

        private Library.IndexerDeclarationInfo _handle;

        private ITranslationUnitItemFactory _itemFactory;

        private EntityInfo _entityInfo;
        private Cursor _cur;
        private SourceLocation _location;
        private Cursor _lexicalContainer;
        private Cursor _semanticContainer;
        private Cursor _declAsContainer;

        #endregion

        #region Constructor

        internal unsafe DeclInfo(Library.IndexerDeclarationInfo handle, ITranslationUnitItemFactory itemFactory)
        {
            _handle = handle;
            _itemFactory = itemFactory;

            _entityInfo = new EntityInfo(*_handle.entityInfo, itemFactory);

            if (handle.semanticContainer != (Library.IndexerContainerInfo*)IntPtr.Zero)
                _semanticContainer = _itemFactory.CreateCursor(handle.semanticContainer->cursor);

            if(handle.lexicalContainer != (Library.IndexerContainerInfo*)IntPtr.Zero)
                _lexicalContainer = _itemFactory.CreateCursor(handle.lexicalContainer->cursor);

            if(handle.declAsContainer != (Library.IndexerContainerInfo*)IntPtr.Zero)
                _declAsContainer = _itemFactory.CreateCursor(handle.declAsContainer->cursor);

            Debug.Assert(Location == Cursor.Location);            
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the EntityInfo object for the declared entity.
        /// </summary>
        public EntityInfo EntityInfo
        {
            get { return _entityInfo; }
        }

        /// <summary>
        /// Returns the Cursor object representing the declaration.
        /// </summary>
        public Cursor Cursor
        {
            get { return _cur ?? (_cur = _itemFactory.CreateCursor(_handle.cursor)); }
        }

        /// <summary>
        /// Returns the Location of the declaration.
        /// </summary>
        public SourceLocation Location
        {
            get { return _location ?? (_location = _itemFactory.CreateSourceLocation(Library.clang_indexLoc_getCXSourceLocation(_handle.location))); }
        }

        /// <summary>
        /// Returns the Cursor object that represents the declaration's lexical container.
        /// </summary>
        public Cursor LexicalContainer
        {
            get { return _lexicalContainer; }
        }

        /// <summary>
        /// Returns the Cursor object that represents the declaration's semantic container.
        /// </summary>
        public Cursor SemanticContainer
        {
            get { return _semanticContainer; }
        }

        public Cursor DeclarationAsContainer
        {
            get { return _declAsContainer; }
        }

        /// <summary>
        /// Retusn true is this declaration is also the definition of the declared entity.
        /// </summary>
        public bool IsDefinition
        {
            get { return _cur.IsDefinition; }
        }

        /// <summary>
        /// Retusn true is this declaration is a redefinition of the declared entity.
        /// </summary>
        public bool IsRedefinition
        {
            get { return _handle.isRedeclaration != 0; }
        }

        #endregion
    }
}
