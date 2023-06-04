# Что такое ImoutoRebirth

ImoutoRebirth это решение для организации медиа файлов с уклоном в аниме картинки. Вдохновленный и глубоко интегрированный с такими сайтами, как danbooru.donmai.us, yande.re, gelbooru.com.

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

# Установка

1. Скачать ImoutoRebirth.exe в любую папку
2. Запустить (это самораспаковывающийся 7z архив), он распакает приложение в папку latest, скопировать путь до этой папки
3. Отредактировать configuration.json, заполнив своими данными
4. Запустить Powershell от имени администратора
5. Выполнить в нем команды ниже по очереди:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force;
```

```powershell
cd <путь к папке latest>
``` 

Установит rabbitmq, postgres, .net runtime
```powershell
./install-dependencies.ps1
```

Установит или обновит само приложение
```powershell
./install-update.ps1
```

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

| Parameter                                 | Required | Comment                                                                                                                                                                                               |
|-------------------------------------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| RabbitMqUrl                               | *        | RabbitMQ host                                                                                                                                                                                         |
| RabbitMqUsername                          | *        | RabbitMQ username                                                                                                                                                                                     |
| RabbitMqPassword                          | *        | RabbitMQ password                                                                                                                                                                                     |
| DanbooruLogin                             |          | Крайне желательно заполнить, используется для поиска тегов и скачивания любимых картинок                                                                                                              |
| DanbooruApiKey                            |          | Крайне желательно заполнить, находится на странице вашего профиля на danbooru, строчка API Key                                                                                                        |
| SankakuLogin                              |          | Опционально, логин с санкаку                                                                                                                                                                          |
| SankakuPassHash                           |          | Опционально, sankaku pass_hash, его можжно найти в куки, после того, как вы залогинетесь на их старом сайте                                                                                           |
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

