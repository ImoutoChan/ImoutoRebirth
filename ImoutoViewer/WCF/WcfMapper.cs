using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImoutoViewer.Model;
using Imouto.WCFExchageLibrary.Data;

namespace ImoutoViewer.WCF
{
    static class WcfMapper
    {
        public static NoteM MapNote(Note note)
        {
            return new NoteM(note.Id, note.NoteString, note.PositionX, note.PositionY, note.Width, note.Height);
        }
    }
}
