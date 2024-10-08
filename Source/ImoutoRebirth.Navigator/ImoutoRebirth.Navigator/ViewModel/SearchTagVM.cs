﻿using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.ViewModel;

internal class SearchTagVM : ObservableObject
{
    private readonly SearchTag _model;

    public SearchTagVM(SearchTag model) => _model = model;

    public Tag Tag => _model.Tag;
        
    public Brush TypeBrush => new SolidColorBrush(Tag.Type.Color.ToColor());

    public string? Value
    {
        get => _model.Value;
        set
        {
            _model.Value = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ValueView));
        }
    }

    public string? ValueView 
        => string.IsNullOrWhiteSpace(Value) 
            ? null 
            : "= " + (Value?.StartsWith('=') == true ? Value?[1..] : Value);

    public string Hint => $"{Tag.Title} : {Value}";

    public SearchType SearchType
    {
        get => _model.SearchType;
        set
        {
            _model.SearchType = value;
            OnPropertyChanged();
        }
    }

    public string Synonyms
        => Tag.SynonymsCollection != null && Tag.SynonymsCollection.Any()
            ? string.Join(", ", Tag.SynonymsCollection)
            : "none";

    public SearchTag Model => _model;
}
