<p align="center"><a href="https://github.com/ImoutoChan/ImoutoRebirth/blob/master/README.md"><b>EN</b></a> | <b>RU</b></p>

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
- [Internal Configuration](#internal-configuration)
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
2. Запустить (это самораспаковывающийся 7z архив), он распакует приложение
3. Запустить файл install.cmd для начала процесса установки
4. Он проверит установлены ли необходимые зависимости, и если нет, то установит их позже
5. Заполнить секции конфигурации для бур и exhentai (если планируете сохранять додзинси), хотя это не обязательно, приложение работает лучше с ними. Я рекомендую заполнить хотя бы danbooru и yandere логины пароли и путь до source вашей основной коллекции. Но в худшем случае, можете оставить все как есть, приложение не сломается, но не будут работать некоторые функции.
    - [Как найти данные аккаунтов?](https://github.com/ImoutoChan/ImoutoRebirth/blob/master/Docs/How%20to%20find%20my%20accounts%20login%20info.md)
6. Оставить все остальное по умолчанию, нажать установить и ждать (установка postgres может занять до 10 минут)
7. Опционально, можно установить расширение для Chrome или Firefox, которое будет подсвечивать сохраненные или связанные с сохраненными картинки на бурах. Ссылки: [хром](https://chrome.google.com/webstore/detail/imouto-extension/ieilellpakdngfomipoedkgfaeddfffc) [файрфокс](https://addons.mozilla.org/en-US/firefox/addon/imouto-extension/)

**Видео гайд с первым запуском, настройкой коллекции, демонстрацией некоторых функций и расширения Imouto Extensions**: https://youtu.be/w7hyAMkQFlU

Старый видео гайд с установкой и начальной конфигурацией. Он устарел, но все еще может дать полезные инсайты: https://youtu.be/xU5lRX-a8kE

# Internal Configuration

## configuration.json

Шаблон и значения по умолчанию

```json
{
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
  "MassTransitConnectionString": "Server=localhost;Port=5432;Database=masstransit;User Id=postgres;Password=postgres;",
  "MeidoMetadataActualizerRepeatEveryMinutes": "5",
  "MeidoFaultToleranceRepeatEveryMinutes": "10080",
  "MeidoFaultToleranceIsEnabled": "true",
  "InstallLocation": "C:\\Program Files\\Imouto",
  "OpenSearchUri": "http://localhost:9200/"
}
```

| Parameter                                 | Required | Comment |
|-------------------------------------------|----------|---------|
| DanbooruLogin                             |          | Крайне желательно заполнить, используется для поиска тегов и скачивания любимых картинок |
| DanbooruApiKey                            |          | Крайне желательно заполнить, находится на странице вашего профиля на danbooru, строчка API Key |
| SankakuLogin                              |          | Опционально, логин с санкаку |
| SankakuPassword                           |          | Опционально, пароль с санкаку |
| YandereLogin                              |          | Крайне желательно заполнить, используется для поиска тегов и скачивания любимых картинок |
| YandereApiKey                             |          | Крайне желательно заполнить, находится на странице Profile / Settings |
| RoomPort                                  | *        | Room сервис будет доступен по этому порту |
| LilinPort                                 | *        | Lilin сервис будет доступен по этому порту |
| KekkaiPort                                | *        | Kekkai сервис будет доступен по этому порту |
| HarpySavePath                             |          | Опционально, сервис будет автоматически скачивать ваши лайки с настроенных бур по этому пути. Если укажете путь на одну из source папок коллекции, то лайки будут автоматически попадать в коллекцию |
| HarpyFavoritesSaveJobRepeatEveryMinutes   | *        | Интервал в минутах, через который сервис будет проверять новые лайки |
| KekkaiAuthToken                           | *        | Случайная строка (можно сгенерить новый guid) |
| LilinConnectionString                     | *        | Строка подключения к postgres для сервиса Lilin |
| MeidoConnectionString                     | *        | Строка подключения к postgres для сервиса Meido |
| RoomConnectionString                      | *        | Строка подключения к postgres для сервиса Room |
| MassTransitConnectionString               | *        | Строка подключения к postgres для транспорта MassTransit (замена rabbit mq) |
| MeidoMetadataActualizerRepeatEveryMinutes | *        | Meido будет запрашивать актуализацию от Danbooru и Yandere через указанный интервал |
| MeidoFaultToleranceRepeatEveryMinutes     | *        | Meido будет запрашивать теги для проблемных файлов через указанный интервал |
| MeidoFaultToleranceIsEnabled              | *        | Meido будет повторять запрос тегов для проблемных файлов |
| InstallLocation                           | *        | Папка установки ImoutoRebirth |
| OpenSearchUri                             |          | Опционально, логирование в open search |
| ExHentaiIpbMemberId                       |          | Опционально, значение куки ExHentai ipbMemberId |
| ExHentaiIpbPassHash                       |          | Опционально, значение куки ExHentai ipbPassHash |
| ExHentaiIgneous                           |          | Опционально, значение куки ExHentai igneous |
| ExHentaiUserAgent                         |          | Опционально, user agent для запросов к ExHentai |
| GelbooruApiKey                            |          | Опционально, ваш API ключ Gelbooru |
| GelbooruUserId                            |          | Опционально, ваш ID пользователя Gelbooru |

## Как устроен ImoutoRebirth

По мимо двух приложений выше, внутри ImoutoRebirth представляет из себя пачку сервисов, каждый из которых имеет свою функцию. Они будут установлены автоматически, и особые знания про них не требуются, но для любопытных глаз я тут опишу внутреннее устройство и назначения разных сервисов, а так же их зависимости.
Если вы будете следовать инструкции по установке выше, эти сервисы установятся автоматически как Windows Service. Так же минимально для работоспособности системы необходим будет Postgres. Вы можете уставновить его самостоятельно, или воспользоваться скриптами, которые прилагаются к каждому релизу.

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
[Расширение для хрома](https://chrome.google.com/webstore/detail/imouto-extension/ieilellpakdngfomipoedkgfaeddfffc) | [Расширение для файрфокса](https://addons.mozilla.org/en-US/firefox/addon/imouto-extension/), которые на бурах подсвечивают сохраненные или связанные изображения.
