using System;
using BookLib;
using System.Collections.Generic;


namespace TcpServer
{
    public sealed class Singleton
    {
        private static readonly object Instancelock = new object();
        private Singleton() { }
        private static Singleton instance = null;

        public static Singleton GetInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (Instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new Singleton();
                        }
                    }
                }
                return instance;
            }
        }

        public List<Book> BooksList = new List<Book>
        {
            new Book("Book Title", "Author Name", 200, "1111111111111"),
            new Book("Book Title 1", "Author Name 1", 21, "1111111111112"),
            new Book("Book Title 2", "Author Name 2", 440, "1111111111123"),
            new Book("Book Title 3", "Author Name 3", 10, "1111111111234"),
            new Book("Book Title 4", "Author Name 4", 800, "1111111112345"),
            new Book("Book Title 5", "Author Name 5", 80, "1111111123456")
        };
    }

}
