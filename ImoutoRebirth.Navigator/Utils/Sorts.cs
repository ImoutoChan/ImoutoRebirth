using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ImoutoRebirth.Navigator.Utils
{
    public static class Sorts
    {
        /// <summary>
        /// Сортирует массив в порядке 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">Коллекция для сортировки</param>
        /// <param name="sortTemplate">Шаблон сортировки</param>
        /// <returns>Возвращает информацию об изменении</returns>
        /// <exception cref="ArgumentNullException"/>
        public static ReportSortList<T> SortList<T>(this ObservableCollection<T> array, IEnumerable<T> sortTemplate)
        {
            if (array == null || sortTemplate == null)
                throw new ArgumentNullException("Нельзя передавать методу SortList пустой параметр");


            //bringing the collection - более лучшее название
            var report = new ReportSortList<T>();
            int index_new = 0;
            try
            {

                List<T> forDelete = new List<T>();
                foreach (T item in array)
                {
                    bool exist = sortTemplate.Contains(item);
                    if (!exist)
                        forDelete.Add(item);
                }
                for (int iDel = 0; iDel < forDelete.Count; iDel++)
                {
                    array.Remove(forDelete[iDel]);
                    report.ReportRemove(forDelete[iDel]);
                }
                foreach (var item in sortTemplate)
                {
                    int index_old = array.IndexOf(item);

                    if (index_old != -1)
                    {
                        if (index_old != index_new)
                        {
                            array.Move(index_old, index_new);
                            report.ReportMove(item);
                        }
                        // else
                        // элемент стоит правильно
                    }
                    else
                    {
                        report.ReportAdd(item);
                        array.Insert(index_new, item);
                    }
                    index_new++;
                }
            }
            catch
            {
                return report;
            }
            return report;
        }

        /// <summary>
        /// Сортирует массив в порядке 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">Коллекция для сортировки</param>
        /// <param name="sortTemplate">Шаблон сортировки</param>
        /// <returns>Возвращает информацию об изменении</returns>
        /// <exception cref="ArgumentNullException"/>
        public static ReportSortList<T> SortList<T>(this IList<T> array, IEnumerable<T> sortTemplate)
        {
            if (array == null || sortTemplate == null)
                throw new ArgumentNullException("Нельзя передавать методу SortList пустой параметр");

            //bringing the collection - более лучшее название
            var report = new ReportSortList<T>();
            int index_new = 0;

            List<T> forDelete = new List<T>();
            foreach (T item in array)
            {
                bool exist = sortTemplate.Contains(item);
                if (!exist)
                    forDelete.Add(item);
            }
            for (int iDel = 0; iDel < forDelete.Count; iDel++)
            {
                array.Remove(forDelete[iDel]);
                report.ReportRemove(forDelete[iDel]);
            }

            foreach (var item in sortTemplate)
            {
                int index_old = array.IndexOf(item);

                if (index_old != -1)
                {
                    if (index_old != index_new)
                    {
                        array.Remove(item);
                        array.Insert(index_new, item);
                        report.ReportMove(item);
                    }
                    // else
                    // элемент стоит правильно
                }
                else
                {
                    report.ReportAdd(item);
                    array.Insert(index_new, item);
                }
                index_new++;
            }
            return report;
        }

        public class ReportSortList<T>
        {
            #region Поля
            List<T> _add;
            List<T> _move;
            List<T> _remove;
            #endregion // Поля

            #region Коллекции
            public IEnumerable<T> Add
            {
                get { return _add; }
            }
            public IEnumerable<T> Move
            {
                get { return _move; }
            }
            public IEnumerable<T> Remove
            {
                get { return _remove; }
            }
            #endregion // Коллекции

            #region Конструктор
            internal ReportSortList()
            {
                _add = new List<T>();
                _move = new List<T>();
                _remove = new List<T>();
            }
            #endregion // Конструктор

            #region Методы
            public void ReportAdd(T item)
            {
                _add.Add(item);
            }
            public void ReportMove(T item)
            {
                _move.Add(item);
            }
            public void ReportRemove(T item)
            {
                _remove.Add(item);
            }
            #endregion // Методы
        }
    }
}