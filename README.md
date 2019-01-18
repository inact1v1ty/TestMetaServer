# Example server to serve meta for [Expload](https://expload.com) marketplace [[RUS](#Пример-мета-сервера-для-торговой-площадки-Expload)]

## Breif introduction to **Meta**

Meta server is used to serve **meta** data of items from your game to Expload marketplace.
**Meta** should be in `json` with such structure:

```json
{
    "name": <itemName>,
    "description": <itemShortDescription>,
    "pictureUrl":  <itemPictureURL>,
    "previewPictureUrl": <itemPreviewPictureUrl>,
    "misc": <miscData>
}
```

where `<miscData>` is a json key-value object with both keys and values as strings.

There are two kinds of **meta** - class and instance.
For example, you can enchant swords in your game and they get a *modifier*. UserA has a normal sword and UserB has an enchanted sword of the same kind. Then, both UserA's sword and UserB's sword will have same class **meta**, but different instance **meta**.

Class **meta** *must* have all fields set: you can have `"description": ""` but not `"description": null` or no `"description"` at all.

Instance **meta** can only have fields that are different from class one - it will override class **meta**. Fields that are not specified in item's instance **meta** will be taken from it's class **meta**.

So, resulting item's **meta** will be formed as:

1. For `name`, `description`, `pictureUrl` and `previewPictureUrl`:
   * Take value from class **meta**.
   * If instance **meta** has this value, override it.
2. `misc`:
   * For each key, value in class `miscData`
      * Take value from class **meta**.
      * If instance **meta** has this key, take its value and override class one.
   * If instance **meta**'s `miscData` has keys that are not in class **meta**, take all such key, value pairs to resulting **meta**.

## Server architecture

This example is built using [Nancy](https://github.com/NancyFx/Nancy) as main framework and [LiteDB](https://github.com/mbdavid/LiteDB) as data storage.

The main file for you is [MetaModule.cs](TestMetaServer/MetaModule.cs)

Let's look at it more closely.

First, a Data Transfer Object called **Meta** is defined:

```csharp
public class Meta
{
    [BsonId]
    public string MetaId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PictureUrl { get; set; }
    public string PreviewPictureUrl { get; set; }
    public Dictionary<string, string> Misc { get; set; }
}
```

`[BsonId]` is used by LiteDB to determine primary key.

Then, the module itself is defined:

```csharp
public class MetaModule : NancyModule
{
    ...
}
```

It's a [Nancy Module](https://github.com/NancyFx/Nancy/wiki/Introduction) - so it will be super-easy for us to define an API

API itself is specified in the constructor:

```csharp
Get["/class-meta/{metaId}"] = parameters =>
{
    ...
};
```

So, class **meta** will be served at `ip:port/class-meta/{metaId}`

Let's go through the code step by step

I get `metaId` parameter:

```csharp
string metaId = parameters.metaId;
```

Using LiteDB's database:

```csharp
using (var db = new LiteDatabase(@"Meta.db"))
{
    ...
}
```

I get **Meta** DTO by id:

```csharp
var meta = db.GetCollection<Meta>("ClassMeta").FindById(metaId);
```

If there's no such **meta**, I return `{}` (not returning `null` because it will be an empty response, not `{}`, and this is not we want):

```csharp
if (meta == null)
    return this.Response.AsJson(new { });
```

Then I form response from DTO.
LiteDB sets `""` to `null` so I fix that, and, I add `pictureUrlBase` to picture URLs in order to have absolute values there.

```csharp
var metaForJson = new
{
    Name = meta.Name,
    Description = meta.Description ?? "",
    PictureUrl = Settings.Default.pictureUrlBase + meta.PictureUrl,
    PreviewPictureUrl = Settings.Default.pictureUrlBase + meta.PreviewPictureUrl,
    Misc = meta.Misc
};
```

Then I just send the result as `json`.

```csharp
return this.Response.AsJson(metaForJson);
```

The same is for instance **meta**, except that in this case I don't set `null` to `""` and leave `PictureUrl` and `PreviewPictureUrl` as `null` if they are:

```csharp
var metaForJson = new
{
    Name = meta.Name,
    Description = meta.Description,
    PictureUrl = meta.PictureUrl == null ? null : Settings.Default.pictureUrlBase + meta.PictureUrl,
    PreviewPictureUrl = meta.PreviewPictureUrl == null ? null : Settings.Default.pictureUrlBase + meta.PreviewPictureUrl,
    Misc = meta.Misc
};
```

---

Server also has a bunch of other files, but they are for a small admin panel where you can add and delete class and instance **meta** from database.

So:

* [AdminModule.cs](TestMetaServer/AdminModule.cs) is a Nancy module that controls admin panel's behaviour.
* [admin/*](TestMetaServer/admin) are static html files used for admin panel.
* [CustomBootstrapper.cs](TestMetaServer/CustomBootstrapper.cs) is a Nancy bootstrapper that allows for images to be served at `images/` instead of `Content/` and also sets up admin panel login.
* [Program.cs](TestMetaServer/Program.cs) is a standard entry point.
* [Settings.settings](TestMetaServer/Settings.settings) is where `pictureUrlBase` is stored.
* [Settings.Designer.cs](TestMetaServer/Settings.Designer.cs) is auto-generated from [Settings.settings](TestMetaServer/Settings.settings)
* [UserIdentity.cs](TestMetaServer/UserIdentity.cs) is `IUserIdentity` realisation.
* [UserMapper.cs](TestMetaServer/UserMapper.cs) is used by admin panel to login.

---

You can start this server and play with it just by opening `TestMetaServer.sln` in Visual Studio and hitting `Start`.

Server will be available at [`http://localhost:9696`](http://localhost:9696/admin)

Login is `admin`, password is `dare-opinion-against-journey`.

<br>
<br>
<br>

# Пример мета-сервера для торговой площадки [Expload](https://expload.com)

## Краткое введение в понятие **Меты**

Мета-сервер используется для того чтобы передавать **мета**-данные предметов из вашей игры в торговую площадку Expload.

**Мета** должна быть в формате `json` с такой структурой:

```json
{
    "name": <itemName>,
    "description": <itemShortDescription>,
    "pictureUrl":  <itemPictureURL>,
    "previewPictureUrl": <itemPreviewPictureUrl>,
    "misc": <miscData>
}
```

где `<miscData>` это json-объект ключ-значение, где и ключи, и значения - строки.

Есть два вида **меты** - class и instance.

Например, вы можете в вашей игре зачаровывать мечи и они получают *модификатор*. У ПользователяА обычный меч, а у ПользователяБ - такой же, но зачарованный. Тогда у меча ПользователяА и у меча ПользователяБ class **мета** будет одинаковой, а instance **мета** - разной.

У class **меты** *должны* быть выставлены все поля: может быть `"description": ""` , но не `"description": null` или совсем не быть `"description"`.

Instance **мета** может иметь только поля, которые отличаются от такив в class **мете** - она перезапишет class **мету**. Поля, которые не указаны в instance **мете** будут взяты из class **меты**.

Таким образом, итоговая **мета** предмета будет сформирована так:
So, resulting item's meta will be formed as:

1. Для `name`, `description`, `pictureUrl` и `previewPictureUrl`:
   * Берется значение из class **меты**.
   * Если в instance **мете** также есть это значение, используется оно.
2. `misc`:
   * Для каждого (ключ, значение) в class `miscData`
      * Взять значение из class **меты**.
      * Если в instance **мете** также есть этот ключ, используется значение из instance **меты**.
   * Если в `miscData` instance **меты** есть ключи которых нет в class **мете**, все такие пары (ключ, значение) попадают в итоговую **мету**.

## Архитектура сервера

Этот пример сделан используя [Nancy](https://github.com/NancyFx/Nancy) как главный фреймворк и [LiteDB](https://github.com/mbdavid/LiteDB) как базу данных.

Главный файл для вас - [MetaModule.cs](TestMetaServer/MetaModule.cs)

Давайте посмотрим на него поближе.

Сначала, объявляется Объект передачи данных (Data Transfer Object, DTO) под названием **Meta**:

```csharp
public class Meta
{
    [BsonId]
    public string MetaId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PictureUrl { get; set; }
    public string PreviewPictureUrl { get; set; }
    public Dictionary<string, string> Misc { get; set; }
}
```

`[BsonId]` используется LitDB чтобы понять, что использовать в качестве основного ключа.

Затем объявляется сам модуль:

```csharp
public class MetaModule : NancyModule
{
    ...
}
```

Это [модуль Nancy](https://github.com/NancyFx/Nancy/wiki/Introduction) - поэтому нам будет очень легко сделать на его основе API.

Само API определяется в конструкторе:

```csharp
Get["/class-meta/{metaId}"] = parameters =>
{
    ...
};
```

Таким образом, class **мета** будет находиться по адресу `ip:port/class-meta/{metaId}`

Давайте пройдемся по коду шаг за шагом

Я получаю параметр `metaId`:

```csharp
string metaId = parameters.metaId;
```

Использую базу данных LiteDB:

```csharp
using (var db = new LiteDatabase(@"Meta.db"))
{
    ...
}
```

Получаю по id объект **Meta**:

```csharp
var meta = db.GetCollection<Meta>("ClassMeta").FindById(metaId);
```

Если такой **меты** в базе данных не нашлось, я возвращаю `{}` (я не возвращаю `null` потому что тогда это преобразуется в пустой ответ, а не `{}`, а это не то, чего мы хотим):

```csharp
if (meta == null)
    return this.Response.AsJson(new { });
```

Затем из DTO я формирую ответ.
LiteDB превращает `""` в `null`, так что я исправляю это, а также добавляю `pictureUrlBase` к путям чтобы они были абсолютными.

```csharp
var metaForJson = new
{
    Name = meta.Name,
    Description = meta.Description ?? "",
    PictureUrl = Settings.Default.pictureUrlBase + meta.PictureUrl,
    PreviewPictureUrl = Settings.Default.pictureUrlBase + meta.PreviewPictureUrl,
    Misc = meta.Misc
};
```

Затем я просто посылаю ответ как `json`.

```csharp
return this.Response.AsJson(metaForJson);
```

Для instance **меты** все тоже самое, за исключением того, что я не заменяю `null` на `""` и оставляю у `PictureUrl` и `PreviewPictureUrl` значение `null` если оно такое в DTO:

```csharp
var metaForJson = new
{
    Name = meta.Name,
    Description = meta.Description,
    PictureUrl = meta.PictureUrl == null ? null : Settings.Default.pictureUrlBase + meta.PictureUrl,
    PreviewPictureUrl = meta.PreviewPictureUrl == null ? null : Settings.Default.pictureUrlBase + meta.PreviewPictureUrl,
    Misc = meta.Misc
};
```

---

В проекте сервера также находятся несколько других файлов, но они используются для простой админ-панели где вы можете добавлять и удалять class и instance **мету** из базы данных.

Таким образом:

* [AdminModule.cs](TestMetaServer/AdminModule.cs) - модуль Nancy, которые отвечает за работу админ-панели.
* [admin/*](TestMetaServer/admin) - статика, html файлы используемые для админ-панели.
* [CustomBootstrapper.cs](TestMetaServer/CustomBootstrapper.cs) - загрузчик, который позваляет выдавать картинки по пути `images/` вместо `Content/`, а также настраивает логин для админ-панели.
* [Program.cs](TestMetaServer/Program.cs) - стандартная точка входа программы.
* [Settings.settings](TestMetaServer/Settings.settings) - файл настроек, где хранится `pictureUrlBase`.
* [Settings.Designer.cs](TestMetaServer/Settings.Designer.cs) - автосгенерированный из [Settings.settings](TestMetaServer/Settings.settings) файл.
* [UserIdentity.cs](TestMetaServer/UserIdentity.cs) - реализация `IUserIdentity`.
* [UserMapper.cs](TestMetaServer/UserMapper.cs) используется админ-панелью для логина.

---

Вы можете запустить этот сервер и поиграться с ним просто открыв `TestMetaServer.sln` в Visual Studio и нажав `Пуск`.

Сервер будет доступен по адресу [`http://localhost:9696`](http://localhost:9696/admin)

Логин `admin`, пароль `dare-opinion-against-journey`.