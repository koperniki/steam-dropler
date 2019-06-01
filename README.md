# Steam-dropler: Бот для фарма предметов

Основаная идея основана на https://github.com/kokole/SteamItemDropIdler
<br>Основаня реализация осуществленна с помощью https://github.com/SteamRE/SteamKit

На данный момент работатет для аккаунтов с файлами от https://github.com/Jessecar96/SteamDesktopAuthenticator, т.е .maFile

Так же с shared_secret (из тех же maFile)

## Настройка
Для начала ставим .net runtime (https://dotnet.microsoft.com/download/thank-you/dotnet-runtime-2.2.5-windows-hosting-bundle-installer)  или более позднию версию

В директории бота *\Configs\Accounts создаем файлы конфигурации для ботов:
 - Имя файла должно быть именем аккаунта steam, расширение .json
 - Тело бота следующее:
 ```
{
  "Password":"пароль_от_акка",
  "IdleEnable":true, //флаг того что аккаунт должен идлиться, false - бот не будет запускать дроп для этого аккаунта
  "SharedSecret":"SDDDONDPyaBSnIJS0PjDMpImcpE=",//открытый ключ для аутентификации, если null то будет искать .maFile
  "DropConfig":[
    {"Item1":id_игры, "Item2":id_дропа}, 
    {"Item1":id_игры, "Item2":id_дропа},
    ........
    {"Item1":id_игры, "Item2":id_дропа}//до 32 конфигов
  ]
}
```

В директории бота *\Configs редактируем MainConfig.json:
 ```
{
  "maFileFolder": "путь до файлов maFiles",
  "dropHistoryFolder": "директория для склада истории дропа", 
  "parallelCount": 100 // количество одновремено запущенных аккаунтов
}

```
## Как работает

1. Бот каждые 30 секунд проверяет возможность запустить на идлинг новый аккаунт 
1. Если количество работающих аккаунтов меньше числа parallelCount, то выбирается кандидат в  соответсвии с его расписанием. На данный момент расписание самое простое (1 час идлинга на каждые 12 часов)
1. В итоге кандидат выбирается так:
   1. Если флаг IdleEnable выставлен в True
   1. И если аккаунт не идлился последнии 12 часов

По итогу за неделю каждый аккаунт наигрывает для каждой игры по 14 часов

Во время работы в файл конфига пишется информация о последнем запуске. Это стоит учитывать, т.к. если вы будете закрывать бота в тот момент пока аккаунты идлятся, он не запустится до следующего времени по расписанию (т.е через 12 часов). Следовательно у вас останутся аккаунты без наигранного времени.
Чтоб этого избежать и в любом случае запустить фарм на аккаунте в файле конфига удлалите запись LastRun.(позже сделаю в автоматическом режиме)
  
## Фишки
- Все игры на аккаунте запускаются одновременно (до 32 штук)
- Дроп проверяется перед началом фарма и каждые пол часа во время фарма (что бы уменьшит количество запросов к серверу).
- изменен подход к подключению к серверам Steam
  - в версии kokole использовался steam.dll и подключение осуществлялось случайно
  - в текущей версии напрямую выбирается сервера из ~200 серверов 
  - на каждый сервер подключаются до 12 (возможно увеличить) аккаунтов
  - так что одновременное количество ботов может быть 2400 (не проверялось)
  
 ## Фишки уже существующие, осталось впилить
 - [ ] Настройка семейного доступа
 - [ ] Получение списка игр для аккаунта (из сети, а не из конфига)
 - [ ] Получение кода авторизации через почту
 - [ ] Ввод кода вручную (нужно впилить GUI)
 
 ## Фишки, которые надо продумать
 - [ ] Настройка расписания для каджой игры, с возможностью редактирования (в связи с эвентами KF2 например)
 - [ ] Расчет по конфигам расписания оптимального запуска ботов
 - [ ] GUI для отслеживания состояния ботов (истории, ошибок) и взаимодействия с пользователем
 - [ ] Покупка игры, принятие кода, покупка игры в падарок 
 - [ ] Автоматическое создание ботов (почта->мобильный аутентификатор)
 - [ ] Перенос функционала ArchiSteamFarm для передачи шмоток 
 - [ ] Дроп TF2
 
### если будет желание поблагодарить 
yandex:410011375178916

bitcoin:3M2m8hLu9w7Z4fonBESafL8SZPfq5SRYBC

qiwi.com/p/79234293663



  
