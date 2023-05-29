using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Commands;

internal record FavoritesSaveCommand : ICommand<bool>;
