﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.EntityFrameworkCore;

using Book_Store.Models;
using System.Text.RegularExpressions;

namespace Book_Store
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _connectionString;
        public MainWindow()
        {
            InitializeComponent();
            LogEntryList.ItemsSource = LogEntryLoggerProvider.LogEntites;

            //var connectionWindow = new ConnectionWindow();
            //var result = connectionWindow.ShowDialog();

            //if (result == true)
            //    _connectionString = connectionWindow.ConnectionString;
            //else
            //    this.Close();

            // for develop
            _connectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={Properties.ConnectionWindowStrings.ServerDatabaseNameDefault};"
                + $@"User id=bookadmin;Password=bookadmin";
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // skip bubbling event
            if (e.Source is not TabControl)
                return;

            string header = (tabControl.SelectedItem as TabItem).Header.ToString();

            if (header == Properties.MainWindowStrings.TabItemBooks)
            {
                UpdateBooks();
            }
            else if (header == Properties.MainWindowStrings.TabItemAuthors)
            {
                UpdateAuthors();
            }
            else if (header == Properties.MainWindowStrings.TabItemPublisher)
            {
                UpdatePublishers();
            }
            else if (header == Properties.MainWindowStrings.TabItemGenre)
            {
                UpdateGenres();
            }

        }
        private void UpdateAuthors()
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                listViewAuthors.ItemsSource = (from author in db.Authors
                                               orderby author.FirstName, author.LastName
                                               select author).ToList();
            }

            firstNameText.Text = string.Empty;
            lastNameText.Text = string.Empty;
            patronymicText.Text = string.Empty;
        }

        private void UpdateBooks()
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                listViewBooks.ItemsSource = (from book in db.Books.Include(nameof(Author)).Include(nameof(Genre)).Include(nameof(Publisher))
                                             orderby book.Name
                                             select book).ToList();

                nameBookText.Text = string.Empty;

                authorComboBox.ItemsSource = db.Authors.ToList();
                authorComboBox.SelectedIndex = 0;

                publisherCheckBox.IsChecked = false;
                publisherComboBox.IsEnabled = false;
                publisherComboBox.ItemsSource = db.Publishers.ToList();
                publisherComboBox.SelectedIndex = -1;

                pagesBookText.Text = string.Empty;

                genreCheckBox.IsChecked = false;
                genreComboBox.IsEnabled = false;
                genreComboBox.ItemsSource = db.Genres.ToList();
                genreComboBox.SelectedIndex = -1;

                yearPublishingText.Text = string.Empty;
                costPriceText.Text = string.Empty;
                priceText.Text = string.Empty;

                previousBookCheckBox.IsChecked = false;
                previousBookComboBox.ItemsSource = db.Books.ToList();
            }
        }

        private void UpdatePublishers()
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                listViewPublisher.ItemsSource = (from publisher in db.Publishers
                                                 orderby publisher.Name
                                                 select publisher).ToList();
            }
            publisherNameText.Text = string.Empty;
        }

        private void UpdateGenres()
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                listViewGenre.ItemsSource = (from genre in db.Genres
                                             orderby genre.Name
                                             select genre).ToList();
            }
            genreNameText.Text = string.Empty;
        }

        // ---------------------------------------------------------------------------------------- //


        private void listViewAuthors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewAuthors.SelectedIndex == -1)
                return;

            var author = listViewAuthors.SelectedItem as Author;

            firstNameText.Text = author.FirstName;
            lastNameText.Text = author.LastName ?? string.Empty;
            patronymicText.Text = author.Patronymic ?? string.Empty;
        }

        private void listViewBooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewBooks.SelectedIndex == -1)
                return;

            var book = listViewBooks.SelectedItem as Book;

            nameBookText.Text = book.Name;
            authorComboBox.SelectedItem = book.Author;
            pagesBookText.Text = book.Pages.ToString();
            yearPublishingText.Text = book.YearPublishing.Year.ToString();
            costPriceText.Text = String.Format("{0:0.00}", book.CostPrice);
            priceText.Text = String.Format("{0:0.00}", book.Price);

            if (book.Publisher is not null)
            {
                publisherCheckBox.IsChecked = true;
                publisherComboBox.SelectedItem = book.Publisher;
            }
            else
            {
                publisherCheckBox.IsChecked = false;
                publisherComboBox.SelectedIndex = -1;
            }

            if (book.Genre is not null)
            {
                genreCheckBox.IsChecked = true;
                genreComboBox.SelectedItem = book.Genre;
            }
            else
            {
                genreCheckBox.IsChecked = false;
                genreComboBox.SelectedIndex = -1;
            }

            // continuation books
            Book previousBook;
            using (var db = new BookStoreContext(_connectionString))
            {
                previousBookComboBox.ItemsSource = (from x in db.Books
                                                    where x.Id != book.Id && x.Id != (from y in db.ContinuationBooks 
                                                                                      where book.Id == y.PredecessorId
                                                                                      select y.BookId).FirstOrDefault()
                                                    select x).ToList();

                previousBook = (from x in previousBookComboBox.ItemsSource.OfType<Book>()
                                where x.Id == (from b in db.ContinuationBooks
                                               where b.BookId == book.Id
                                               select b.PredecessorId).FirstOrDefault()
                                select x).FirstOrDefault();
            }

            if (previousBook is not null)
            {
                previousBookCheckBox.IsChecked = true;
                previousBookComboBox.SelectedItem = previousBook;
            }
            else
            {
                previousBookCheckBox.IsChecked = false;
                previousBookComboBox.SelectedIndex = -1;
            }
        }

        private void listViewPublisher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewPublisher.SelectedIndex == -1)
                return;

            var publisher = listViewPublisher.SelectedItem as Publisher;

            publisherNameText.Text = publisher.Name;
        }

        private void listViewGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewGenre.SelectedIndex == -1)
                return;

            var genre = listViewGenre.SelectedItem as Genre;

            genreNameText.Text = genre.Name;
        }


        // ---------------------------------------------------------------------------------------- //


        private void addAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            if (firstNameText.Text == string.Empty)
            {
                MessageBox.Show("First name is empty!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    db.Authors.Add(new Author()
                    {
                        FirstName = firstNameText.Text != string.Empty ? firstNameText.Text : null,
                        LastName = lastNameText.Text != string.Empty ? lastNameText.Text : null,
                        Patronymic = patronymicText.Text != string.Empty ? patronymicText.Text : null,
                    });

                    db.SaveChanges();
                    UpdateAuthors();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void changeAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            if (firstNameText.Text == string.Empty)
            {
                MessageBox.Show("First name is empty!", Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    var author = db.Authors.First(a => a.Id == (listViewAuthors.SelectedItem as Author).Id);
                    author.FirstName = firstNameText.Text != string.Empty ? firstNameText.Text : null;
                    author.LastName = lastNameText.Text != string.Empty ? lastNameText.Text : null;
                    author.Patronymic = patronymicText.Text != string.Empty ? patronymicText.Text : null;

                    db.SaveChanges();
                    UpdateAuthors();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void deleteAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    db.Authors.Remove(listViewAuthors.SelectedItem as Author);
                    db.SaveChanges();
                    UpdateAuthors();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void addPublisherButton_Click(object sender, RoutedEventArgs e)
        {
            if (publisherNameText.Text == string.Empty)
            {
                MessageBox.Show("publisher name is empty!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    db.Publishers.Add(new Publisher()
                    {
                        Name = publisherNameText.Text
                    });

                    db.SaveChanges();
                    UpdatePublishers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void changePublisherButton_Click(object sender, RoutedEventArgs e)
        {
            if (publisherNameText.Text == string.Empty)
            {
                MessageBox.Show("publisher name is empty!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    var publisher = db.Publishers.First(a => a.Id == (listViewPublisher.SelectedItem as Publisher).Id);
                    publisher.Name = publisherNameText.Text;

                    db.SaveChanges();
                    UpdatePublishers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void deletePublisherButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    db.Publishers.Remove(listViewPublisher.SelectedItem as Publisher);
                    db.SaveChanges();
                    UpdatePublishers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void addGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (genreNameText.Text == string.Empty)
            {
                MessageBox.Show("genre name is empty!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    db.Genres.Add(new Genre()
                    {
                        Name = genreNameText.Text
                    });

                    db.SaveChanges();
                    UpdateGenres();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void changeGenreButton_Click(object sender, RoutedEventArgs e)
        {
            if (genreNameText.Text == string.Empty)
            {
                MessageBox.Show("genre name is empty!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    var genre = db.Genres.First(a => a.Id == (listViewGenre.SelectedItem as Genre).Id);
                    genre.Name = genreNameText.Text;

                    db.SaveChanges();
                    UpdateGenres();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void deleteGenreButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    db.Genres.Remove(listViewGenre.SelectedItem as Genre);
                    db.SaveChanges();
                    UpdateGenres();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void addBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckTextBoxsOfBook())
                return;

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    var newBook = db.Books.Add(new Book()
                    {
                        Name = nameBookText.Text,
                        AuthorId = (authorComboBox.SelectedItem as Author).Id,
                        PublisherId = publisherCheckBox.IsChecked == true ? (publisherComboBox.SelectedItem as Publisher).Id : null,
                        Pages = int.Parse(pagesBookText.Text),
                        GenreId = genreCheckBox.IsChecked == true ? (genreComboBox.SelectedItem as Genre).Id : null,
                        YearPublishing = new DateTime(int.Parse(yearPublishingText.Text), 1, 1),
                        CostPrice = decimal.Parse(costPriceText.Text),
                        Price = decimal.Parse(priceText.Text),
                    });

                    db.ContinuationBooks.Add(new ContinuationBook()
                    {
                        Book = newBook.Entity, 
                        PredecessorId = (previousBookComboBox.SelectedItem as Book).Id,
                    });

                    db.SaveChanges();
                    UpdateBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void changeBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckTextBoxsOfBook())
                return;

            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    var book = db.Books.First(x => x.Id == (listViewBooks.SelectedItem as Book).Id);

                    book.Name = nameBookText.Text;
                    book.AuthorId = (authorComboBox.SelectedItem as Author).Id;
                    book.PublisherId = publisherCheckBox.IsChecked == true ? (publisherComboBox.SelectedItem as Publisher).Id : null;
                    book.Pages = int.Parse(pagesBookText.Text);
                    book.GenreId = genreCheckBox.IsChecked == true ? (genreComboBox.SelectedItem as Genre).Id : null;
                    book.YearPublishing = new DateTime(int.Parse(yearPublishingText.Text), 1, 1);
                    book.CostPrice = decimal.Parse(costPriceText.Text);
                    book.Price = decimal.Parse(priceText.Text);

                    var continuationBook = (from x in db.ContinuationBooks where book.Id == x.BookId select x).FirstOrDefault();
                    if (previousBookCheckBox.IsChecked == true)
                    {
                        if (continuationBook is not null)
                        {
                            continuationBook.PredecessorId = (previousBookComboBox.SelectedItem as Book).Id;
                        }
                        else
                        {
                            db.ContinuationBooks.Add(new ContinuationBook()
                            {
                                Book = book,
                                PredecessorId = (previousBookComboBox.SelectedItem as Book).Id,
                            });
                        }
                    }
                    else
                    {
                        if (continuationBook is not null)
                            db.ContinuationBooks.Remove(continuationBook);
                    }

                    db.SaveChanges();
                    UpdateBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void deleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BookStoreContext(_connectionString))
            {
                try
                {
                    db.Books.Remove(listViewBooks.SelectedItem as Book);
                    db.SaveChanges();
                    UpdateBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), Properties.MainWindowStrings.WindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // ---------------------------------------------------------------------------------------- //


        private bool CheckTextBoxsOfBook()
        {
            if (nameBookText.Text == string.Empty)
            {
                MessageBox.Show("book name is empty!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (pagesBookText.Text == string.Empty)
            {
                MessageBox.Show("pages field is empty!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (int.Parse(yearPublishingText.Text) < 1 || int.Parse(yearPublishingText.Text) > 9999)
            {
                MessageBox.Show("year of publishing is not correct!", Properties.MainWindowStrings.WindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }


        // ---------------------------------------------------------------------------------------- //


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void FloatValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void genreCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            genreComboBox.IsEnabled = true;
        }

        private void publisherCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            publisherComboBox.IsEnabled = true;
        }

        private void publisherCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            publisherComboBox.IsEnabled = false;
        }

        private void genreCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            genreComboBox.IsEnabled = false;
        }

        private void previousBookCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            previousBookComboBox.IsEnabled = true;
        }

        private void previousBookCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            previousBookComboBox.IsEnabled = false;
        }

        private void updateDbBookButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateBooks();
        }
    }
}
