using System.Drawing;

namespace ClassifiedDocumentsComparer
{
    /// <summary>
    /// Klasa zawierajaca możliwe typy oznaczen dokumentu.
    /// </summary>
    public static class DocumentClasses
    {
        /// <summary>
        /// Pieczatka
        /// </summary>
        public static (string Name, Color Color) Stamp => ("Stamp", Color.Green);
        /// <summary>
        /// Tekst drukowany.
        /// </summary>
        public static (string Name, Color Color) Text => ("Text", Color.Blue);
        /// <summary>
        /// Pieczatka/Obrazek.
        /// </summary>
        public static (string Name, Color Color) Sign => ("Sign", Color.Orange);
        /// <summary>
        /// Tabela.
        /// </summary>
        public static (string Name, Color Color) Table => ("Table", Color.Pink);
        /// <summary>
        /// Data.
        /// </summary>
        public static (string Name, Color Color) Data => ("Data", Color.Purple);
    }
}