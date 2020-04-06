// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ImoutoRebirth.Room.WebApi.Client
{
    using Models;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for SourceFolders.
    /// </summary>
    public static partial class SourceFoldersExtensions
    {
            /// <summary>
            /// Get all source folders for collection.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id.
            /// </param>
            public static IList<SourceFolderResponse> GetAll(this ISourceFolders operations, System.Guid collectionId)
            {
                return operations.GetAllAsync(collectionId).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get all source folders for collection.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<SourceFolderResponse>> GetAllAsync(this ISourceFolders operations, System.Guid collectionId, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetAllWithHttpMessagesAsync(collectionId, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Create a source folder for collection.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id.
            /// </param>
            /// <param name='body'>
            /// Source folder parameters.
            /// </param>
            public static SourceFolderResponse Create(this ISourceFolders operations, System.Guid collectionId, SourceFolderCreateRequest body = default(SourceFolderCreateRequest))
            {
                return operations.CreateAsync(collectionId, body).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Create a source folder for collection.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id.
            /// </param>
            /// <param name='body'>
            /// Source folder parameters.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<SourceFolderResponse> CreateAsync(this ISourceFolders operations, System.Guid collectionId, SourceFolderCreateRequest body = default(SourceFolderCreateRequest), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateWithHttpMessagesAsync(collectionId, body, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Update the source folder for given collection.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id. Aren't needed and added only for routes consistency.
            /// </param>
            /// <param name='sourceFolderId'>
            /// The id of the source folder that will be updated.
            /// </param>
            /// <param name='body'>
            /// Source folder parameters.
            /// </param>
            public static SourceFolderResponse Update(this ISourceFolders operations, System.Guid collectionId, System.Guid sourceFolderId, SourceFolderCreateRequest body = default(SourceFolderCreateRequest))
            {
                return operations.UpdateAsync(collectionId, sourceFolderId, body).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Update the source folder for given collection.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id. Aren't needed and added only for routes consistency.
            /// </param>
            /// <param name='sourceFolderId'>
            /// The id of the source folder that will be updated.
            /// </param>
            /// <param name='body'>
            /// Source folder parameters.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<SourceFolderResponse> UpdateAsync(this ISourceFolders operations, System.Guid collectionId, System.Guid sourceFolderId, SourceFolderCreateRequest body = default(SourceFolderCreateRequest), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.UpdateWithHttpMessagesAsync(collectionId, sourceFolderId, body, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Delete the source folder.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id. Aren't needed and added only for routes consistency.
            /// </param>
            /// <param name='sourceFolderId'>
            /// Id of the folder that will be deleted.
            /// </param>
            public static void Delete(this ISourceFolders operations, System.Guid collectionId, System.Guid sourceFolderId)
            {
                operations.DeleteAsync(collectionId, sourceFolderId).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Delete the source folder.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='collectionId'>
            /// The collection id. Aren't needed and added only for routes consistency.
            /// </param>
            /// <param name='sourceFolderId'>
            /// Id of the folder that will be deleted.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task DeleteAsync(this ISourceFolders operations, System.Guid collectionId, System.Guid sourceFolderId, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.DeleteWithHttpMessagesAsync(collectionId, sourceFolderId, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

    }
}
