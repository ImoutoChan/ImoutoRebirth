# Что такое ImoutoRebirth

ImoutoRebirth это решение для организации медиа файлов с уклоном в аниме картинки. Вдохновленный и глубоко интегрированный с такими сайтами, как danbooru.donmai.us, yande.re, gelbooru.com.

Больше всего ImoutoRebirth будет полезен тем, кто любит сохранять картинки (мноого картинок), а потом иметь возможность в них разобраться и найти нужные, что без тегов обычно бывает сделать крайне проблематично.

Ключевые функции:

- Коллекции файлов (картинок, видосов, гифок)
- Поддерживаются форматы:
    - jpg, jpeg, png, bmp, tiff, apng, gif, webp, jfif
    - mp4, mov, webm, wmv, mov, m2ts, mpg, mpeg, mkv, f4v, flv, avi, m4v, swf
    - zip as pixiv ugoira
- Фоновый поиск и актуализация тегов для ваших картинок и метаданных по хешу файла с данбуры, санкаку, яндере, гелбуру
- Локальный поиск по сохраненным файлам и их тегам
- Массовое тегирование файлов
- Отображение notes с бур на локальных картинках
- Автоматическое скачивание favorites с бур в вашу коллекцию
- Подсвечивание в браузере сохраненных картинок

## Как выглядит ImoutoRebirth

Для вас доступно два приложения, одно для просмотра и настройки коллекций, тегировния. Своебразный explorer.

### ImoutoRebirth Navigator



# Installation

1. Download ImoutoRebirth.exe to any folder
2. Run it (it self extracting 7z archive), it will extract the program to latest folder
3. Start Powershell as Administrator
4. Edit configuration.json and fill with your data
5. Run commands in the following order:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force;
```

```powershell
cd "path to the extracted latest folder"
``` 

```powershell
./install-dependencies.ps1
```

```powershell
./install-update.ps1
```

# Configuration

## configuration.json

Template and default values

```json
{
  "RabbitMqUrl": "rabbitmq://localhost:5672",
  "RabbitMqUsername": "guest",
  "RabbitMqPassword": "guest",
  "DanbooruLogin": "",
  "DanbooruApiKey": "",
  "SankakuLogin": "",
  "SankakuPassHash": "",
  "YandereLogin": "",
  "YandereApiKey": "",
  "RoomPort": "11301",
  "LilinPort": "11302",
  "KekkaiPort": "11303",
  "HarpySavePath": "",
  "HarpyFavoritesSaveJobRepeatEveryMinutes": "1",
  "KekkaiAuthToken": "",
  "LilinConnectionString": "Server=localhost;Port=5432;Database=LilinProd;User Id=postgres;Password=postgres;",
  "MeidoConnectionString": "Server=localhost;Port=5432;Database=MeidoProd;User Id=postgres;Password=postgres;",
  "RoomConnectionString": "Server=localhost;Port=5432;Database=RoomProd;User Id=postgres;Password=postgres;",
  "MeidoMetadataActualizerRepeatEveryMinutes": "5",
  "MeidoFaultToleranceRepeatEveryMinutes": "10080",
  "MeidoFaultToleranceIsEnabled": "true",
  "RoomImoutoPicsUploadUrl": "",
  "InstallLocation": "C:\\Program Files\\Imouto",
  "OpenSearchUri": "http://localhost:9200/"
}
```

| Parameter                                 | Required | Comment                                                                                                                                                                                    |
|-------------------------------------------|----------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| RabbitMqUrl                               | *        | RabbitMQ host                                                                                                                                                                              |
| RabbitMqUsername                          | *        | RabbitMQ username                                                                                                                                                                          |
| RabbitMqPassword                          | *        | RabbitMQ password                                                                                                                                                                          |
| DanbooruLogin                             |          | Optional but recommended, only if you want ImoutoRebirth to search tags for your files in danbooru                                                                                         |
| DanbooruApiKey                            |          | Optional but recommended, you can find it at the end of your danbooru profile page, API Key row                                                                                            |
| SankakuLogin                              |          | Optional, your sankaku login                                                                                                                                                               |
| SankakuPassHash                           |          | Optional, your sankaku pass_hash, you can find this value in cookies after login on their old site                                                                                         |
| YandereLogin                              |          | Optional but recommended, only if you want ImoutoRebirth to search tags for your files in yandere                                                                                          |
| YandereApiKey                             |          | Optional but recommended, you can find it in Profile / Settings                                                                                                                            |
| RoomPort                                  | *        | Room service will be exposed through this port                                                                                                                                             |
| LilinPort                                 | *        | Lilin service will be exposed through this port                                                                                                                                            |
| KekkaiPort                                | *        | Kekkai service will be exposed through this port                                                                                                                                           |
| HarpySavePath                             |          | Optional, Service will automatically save your likes from configured boorus to this path. If you point it to your collection from Room, you will get liked images from booru automatically |
| HarpyFavoritesSaveJobRepeatEveryMinutes   | *        | Interval in minutes, in which the service will check for new liked images                                                                                                                  |
| KekkaiAuthToken                           | *        | Place here random string (you can generate new guid or something)                                                                                                                          |
| LilinConnectionString                     | *        | Connection string for Lilin service to the postgres database                                                                                                                               |
| MeidoConnectionString                     | *        | Connection string for Meido service to the postgres database                                                                                                                               |
| RoomConnectionString                      | *        | Connection string for Room service to the postgres database                                                                                                                                |
| MeidoMetadataActualizerRepeatEveryMinutes | *        | Meido service will request actualization from Danbooru and Yandere in specified interval                                                                                                   |
| MeidoFaultToleranceRepeatEveryMinutes     | *        | Meido service will request tags for failed files in specified interval                                                                                                                     |
| MeidoFaultToleranceIsEnabled              | *        | Meido service will repeat tag request for failed file                                                                                                                                      |
| RoomImoutoPicsUploadUrl                   |          | Optional, callback that will be called for every saved file                                                                                                                                |
| InstallLocation                           | *        | Installation location for ImoutoRebirth                                                                                                                                                    |
| OpenSearchUri                             |          | Optional, logging to open search                                                                                                                                                           |

