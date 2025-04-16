using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using ProyectoBD2.DataAccess;
using ProyectoBD2.Models;

namespace ProyectoBD2;

// MainWindow.axaml.cs
public partial class MainWindow : Window
{
    private ObservableCollection<Person> _people;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _people = new ObservableCollection<Person>
        {
            new Person("Juan", "Pérez"),
            new Person("María", "López")
        };
        
        var dataGrid = this.FindControl<DataGrid>("PeopleDataGrid");
        dataGrid.ItemsSource = _people;
        
        var addButton = this.FindControl<Button>("AddButton");
        addButton.Click += OnAddButtonClick;
    }
    
    private void OnAddButtonClick(object? sender, EventArgs e)
    {
        _people.Add(new Person("Nuevo", "Usuario"));
    }
}