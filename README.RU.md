## Важное

Этот репозиторий это исходный код программы, которую написал для себя и использовал многие годы. Инструкции по установке проверены на чистой Windows 10 и должны работать без проблем, но если у вас возникнут какие-либо трудности, не стесняйтесь создавать discussion или issue. Я также планирую в ближайшем будущем загрузить видео с демонстрацией процесса установки, это должно немного помочь.

- [Последний релиз](https://github.com/ImoutoChan/ImoutoRebirth/releases/latest)
- [Быстрый старт](#установка)

## Содержание

- [Что такое ImoutoRebirth](#что-такое-imoutorebirth)
- [Как выглядит ImoutoRebirth](#как-выглядит-imoutorebirth)
  - [Imouto Navigator](#imouto-navigator)
  - [ImoutoRebirth Viewer](#imoutorebirth-viewer)
- [Установка](#установка)
- [Configuration](#configuration)
  - [configuration.json](#configurationjson)
- [Как устроен ImoutoRebirth](#как-устроен-imoutorebirth)
  - [ImoutoRebirth.Room](#imoutorebirthroom)
  - [ImoutoRebirth.Arachne](#imoutorebirtharachne)
  - [ImoutoRebirth.Lilin](#imoutorebirthlilin)
  - [ImoutoRebirth.Meido](#imoutorebirthmeido)
  - [ImoutoRebirth.Harpy](#imoutorebirthharpy)
  - [ImoutoRebirth.Kekkai](#imoutorebirthkekkai)
  - [ImoutoRebirth.Tori](#imoutorebirthtori)
  - [Imouto.Viewer](#imoutoviewer)
  - [Imouto.Extensions](#imoutoextensions)

# Что такое ImoutoRebirth

ImoutoRebirth это решение для организации медиа файлов с уклоном в аниме картинки. Вдохновленный и глубоко интегрированный с такими сайтами, как danbooru.donmai.us, yande.re, gelbooru.com, rule34.xxx.

Писать я его начал для себя много лет назад, т.к. я сам еще тот сохранятель картинок. Со временем разные функции собрались в целостное решение, и я решил его опубликовать.

Больше всего ImoutoRebirth будет полезен тем, кто любит сохранять картинки (мноого картинок), а потом иметь возможность в них разобраться и найти нужные, что без тегов обычно бывает сделать крайне проблематично.

Ключевые функции:

- Коллекции файлов (картинок, видосов, гифок)
- Поддерживаются форматы
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

Для вас доступно два приложения

### Imouto Navigator

Для просмотра и настройки коллекций, тегировния. Своебразный explorer.

![Imouto Navigator](https://raw.githubusercontent.com/ImoutoChan/ImoutoRebirth/master/Docs/Screens/Imouto%20Navigator.png "Imouto Navigator")

### ImoutoRebirth Viewer

Для просмотра отдельных изображений. Его использование не обязательно, вы можете пользоваться своим любимым просмотрщиком картинок, но в Imouto Viewer позволит вам читать Notes с бур (обычно это переводы текста на картинках), смотреть и добавлять свои теги.

![Imouto Navigator](https://raw.githubusercontent.com/ImoutoChan/ImoutoRebirth/master/Docs/Screens/Imouto%20Viewer.png "Imouto Viewer")

# Установка

Основная идея приложения в том, что у вас есть разные коллекции с файлами, где хранятся ваши изображения/видео. После установки вам необходимо настроить эти коллекции в приложении ImoutoNavigator. У каждой коллекции есть source папки и (по желанию) папка destination. Приложение отслеживает каждую source папку, берет файлы оттуда, обрабатывает их и перемещает их в папку destination. После этого файлы можно просматривать в приложении ImoutoNavigator. Установка происходит следующим образом:

1. Скачать ImoutoRebirth.exe в любую папку из [последнего релиза](https://github.com/ImoutoChan/ImoutoRebirth/releases/latest)
2. Запустить (это самораспаковывающийся 7z архив), он распакает приложение в папку с версией, скопировать путь до этой папки
3. Отредактировать configuration.json, заполнив своими данными. Я рекомендую заполнить хотя бы danbooru и yandere логины пароли и путь до source вашей основной коллекции в HarpySavePath. Но в худшем случае, можете оставить все как есть, приложение не сломается, но не будут работать некоторые функции.
4. Запустить Powershell от имени администратора
5. Выполнить в нем команды ниже по очереди:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force;
```

```powershell
cd <путь к папке latest>
``` 

Установить rabbitmq, postgres, .net runtime (может занять несколько минут, особенно долгий Postgres)
```powershell
./install-dependencies.ps1
```

Установить или обновить само приложение
```powershell
./install-update.ps1
```

6. Опционально, можно установить расширение для хрома, которое будет подсвечивать сохраненные или связанные с сохраненными картинки на бурах. [Ссылка на расширение](https://chrome.google.com/webstore/detail/imouto-extension/ieilellpakdngfomipoedkgfaeddfffc)

# Configuration

## configuration.json

Шаблон и значения по умолчанию

```json
{
  "RabbitMqUrl": "rabbitmq://localhost:5672",
  "RabbitMqUsername": "guest",
  "RabbitMqPassword": "guest",
  "DanbooruLogin": "",
  "DanbooruApiKey": "",
  "SankakuLogin": "",
  "SankakuPassword": "",
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

| Parameter                                 | Required | Comment                                                                                                                                                                                               |
|-------------------------------------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| RabbitMqUrl                               | *        | RabbitMQ host                                                                                                                                                                                         |
| RabbitMqUsername                          | *        | RabbitMQ username                                                                                                                                                                                     |
| RabbitMqPassword                          | *        | RabbitMQ password                                                                                                                                                                                     |
| DanbooruLogin                             |          | Крайне желательно заполнить, используется для поиска тегов и скачивания любимых картинок                                                                                                              |
| DanbooruApiKey                            |          | Крайне желательно заполнить, находится на странице вашего профиля на danbooru, строчка API Key                                                                                                        |
| SankakuLogin                              |          | Опционально, логин с санкаку                                                                                                                                                                          |
| SankakuPassword                           |          | Опционально, sankaku password                                                                                                                                                                         |
| YandereLogin                              |          | Крайне желательно заполнить, используется для поиска тегов и скачивания любимых картинок                                                                                                              |
| YandereApiKey                             |          | Крайне желательно заполнить, находится на странице Profile / Settings                                                                                                                                 |
| RoomPort                                  | *        | Room сервис будет доступен по этому порту                                                                                                                                                             |
| LilinPort                                 | *        | Lilin сервис будет доступен по этому порту                                                                                                                                                            |
| KekkaiPort                                | *        | Kekkai сервис будет доступен по этому порту                                                                                                                                                           |
| HarpySavePath                             |          | Опционально, сервис будет скачивать любимые картинки с danbooru и yandere по этому пути. Если вы укажите этот путь на Source папку вашей коллекции, то картинки сразу будут попадать в вашу коллекцию |
| HarpyFavoritesSaveJobRepeatEveryMinutes   | *        | Время в минуту, через которое буры будут проверяться на новые любимые                                                                                                                                 |
| KekkaiAuthToken                           | *        | Случайная строка для безопасности, можно сгенерить, например, новый guid и вставить сюда                                                                                                              |
| LilinConnectionString                     | *        | Строка подключения в постгрес для Lilin                                                                                                                                                               |
| MeidoConnectionString                     | *        | Строка подключения в постгрес для Meido                                                                                                                                                               |
| RoomConnectionString                      | *        | Строка подключения в постгрес для Room                                                                                                                                                                |
| MeidoMetadataActualizerRepeatEveryMinutes | *        | Meido будет запрашивать актуализацию тегов через указанное количество минут                                                                                                                           |
| MeidoFaultToleranceRepeatEveryMinutes     | *        | Meido в случае проблем с сетью, через указанное количество минут будет повторно запрашивать поиск                                                                                                     |
| MeidoFaultToleranceIsEnabled              | *        | Meido нужно ли повторно запрашивать поиск в случае проблем с сетью                                                                                                                                    |
| RoomImoutoPicsUploadUrl                   |          | Опционально, колбек, на который будут отправляться все добавленные картинки                                                                                                                           |
| InstallLocation                           | *        | Папка установки ImoutoRebirth (лучше не трогать)                                                                                                                                                      |
| OpenSearchUri                             |          | Больше для отладки, если вы хотите наблюдать за системой и смотреть ее логи вне файлов, можете поднять OpenSearch в докере и указать здесь его урл. Система начнет логировать в него.                 |

## Как устроен ImoutoRebirth

По мимо двух приложений выше, внутри ImoutoRebirth представляет из себя пачку сервисов, каждый из которых имеет свою функцию. Они будут установлены автоматически, и особые знания про них не требуются, но для любопытных глаз я тут опишу внутреннее устройство и назначения разных сервисов, а так же их зависимости.
Если вы будете следовать инструкции по установке ниже, эти сервисы установятся автоматически как Windows Service. Так же минимально для работоспособности системы необходим будет Postgres и RabbitMQ. Вы можете уставновить их самостоятельно, или воспользоваться скриптами, которые прилагаются к каждому релизу.

Подробнее взаимодействие сервисов и их назначение можно посмотреть на [этой диаграме](https://drive.google.com/file/d/1MD8NAIeuV8u_wt9HWjdUUrcOaiZOMNBF/view?usp=drive_link), открывать с помощью diagrams.net или draw.io.

### ImoutoRebirth.Room
Сервис отвечает за работу с файловой системой. Именно он хранит ваши коллекции, перемещает файлы, проверяет на дубликаты, считает и сверяет md5 хеш. Настройка коллекций из Imouto Navigator вызывает методы именно в нем.

### ImoutoRebirth.Arachne
Сервис отвечает за взаимодействие с внешним миром и сбор тегов и метаданных, асинхронно получая задания (например, когда вы сохраняете новый файл), он лезет в известные ему буры сайты и скачивает с них теги, заметки и метаданные. Он же занимается актуализацией тегов, отслеживая изменения, которые обычно буры публикую в качестве фида. Так что в идеале у вас локально всегда актуальные теги, такие же как на сайтах.

### ImoutoRebirth.Lilin
Сервис отвечает за хранение и поиск тегов, сюда складывается результат работы Arachne и те теги, что вы добавляете сами.

### ImoutoRebirth.Meido
Сервис координатор, который отслеживает процессы, происходящие в системе и ставит задания Arachne на поиск или актуализацию тегов.

### ImoutoRebirth.Harpy
Мини сервис скачивает favs из ваших аккаунтов бур, и складывает их в указанную папку.

### ImoutoRebirth.Kekkai
Апи гейтвей который проксирует запросы на внутренние сервисы и позволяет проверять, сохранены ли определенные картинки в системе. Им пользуется, например, Imouto Extensions.

### ImoutoRebirth.Tori
Не будет запускаться или устанавливаться в вашей системе, это сервис обновлятор на новую версию. 

### Imouto.Viewer
Приложение для просмотра отдельных изображений. Поддерживает теги, заметки (переводы), слайдшоу, фиксированный зум (удобно для чтения манги) и многое другое

### Imouto.Extensions
[Расширение для хрома](https://chrome.google.com/webstore/detail/imouto-extension/ieilellpakdngfomipoedkgfaeddfffc), которое на бурах подсвечивает сохраненные изображения.
