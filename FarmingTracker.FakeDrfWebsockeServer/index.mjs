import { WebSocketServer } from 'ws';
import * as fs from 'fs';

const drfToken = fs.readFileSync("C:\\Dev\\blish\\drftoken_server.txt", 'utf-8');
const webSocketServer = new WebSocketServer({ port: 8080 });

webSocketServer.on('connection', (webSocket, request) => {
  console.log(`connection`);
  webSocket.request = request;

  webSocket.on('error', (error) => console.error(`error: ${error}`));

  webSocket.on('close', (code, reason) => console.error(`close: ${code} ${reason}`));

  webSocket.on('message', async (data) => {
    console.log(`received: ${data}`);    
    const isAuthenticated = data.toString() === `Bearer ${drfToken}`;
    if(isAuthenticated)
      await sendDrop(webSocket);
    else
      closeConnection();
  });
});

async function sendDrop(webSocket) {
  console.log("isAuthenticated YES");

  // todo: datei(en) einlesen stattdessen
  for (const message of messages) {
    console.log(`send to client: ${message}`);
    webSocket.send(message);
    await wait(1000);
  }
}

const messages = [
  // CSV TEST (use online csv validator check if file content is valid csv)
  // items with comma (,) or double quotes ("") in name:
  // Sun, Moon, and Stars; Mask of 1,000 Faces; "Acquired" Shoes; Mini "Knuckles"; Story Unlock: "The Dragon's Reach, Part 2"
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"70671":1,"50210":1,"3973":1,"77345":1,"66870":1},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // NO DROP
  // only items
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19620":1},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19620":1,"96144":1},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // only currencies
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{},"curr":{"1":123456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{},"curr":{"1":123456, "2":-3},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // more items than currencies
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19620":1,"96144":1},"curr":{"1":123456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // more currencies than items
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"85016":1},"curr":{"1":123456, "2":-3},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // same amount of currencies and items
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"85016":1},"curr":{"1":123456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',

  // 47 currencies and 820+ items
  // '{"Kind":"data","Payload":{"Character":"1","Drop":{"items":{"24293":1795,"96722":28,"19739":-1931,"84731":31,"12537":4,"12255":246,"12536":-246,"12248":91356,"12243":-91355,"12538":10,"39264":64,"68633":-3,"68632":-1,"77699":-94,"94694":-1,"68636":-2,"77686":-6,"77750":-24,"68635":-1,"77667":-46,"38128":-7,"77632":-10,"77648":-43,"49297":-1,"77643":-15,"77576":-40,"38061":-5,"38063":-5,"68638":-75,"77567":-12,"77630":-13,"24":-5,"38062":-2,"77569":-16,"68634":-35,"77747":-2,"77579":-9,"24521":17,"24523":10,"45175":-492,"46735":2057,"24522":4,"85016":-185,"21683":4,"24341":204,"71945":-2,"77256":1473,"19732":645,"81479":1,"19701":-1434,"19700":-5751,"43766":-1,"12532":2,"19620":6,"22997":-50,"12247":2,"19719":160,"19575":15,"45178":39,"39233":-2,"80002":-2,"49365":-2,"39273":-2,"37086":-2,"49403":-2,"64142":-2,"24473":11,"24475":8,"96464":11,"71428":-242,"78600":6,"99787":8,"99814":1,"19699":-3716,"81079":1,"44205":1,"46733":2740,"12533":3,"19743":-4845,"19731":-2911,"8920":10,"99004":1,"24277":1440,"24284":369,"24285":244,"24292":1625,"24345":1680,"24350":-180,"24325":6,"24476":11,"24519":6,"83008":-71,"12544":6,"12547":-4,"24359":2,"66923":2,"66933":137,"15475":1,"15433":1,"67178":-1,"98599":-1,"64675":-1,"96144":-5,"24474":13,"44081":-1,"44202":-1,"24320":1,"24339":2,"37794":1,"38915":1,"49424":2374,"75919":37,"77206":1,"79792":40,"86181":2,"77580":1,"68319":1,"72500":2,"20349":-43,"39333":1,"79710":-1,"77890":-1,"49432":-15,"77413":-1,"79745":-1,"46986":1,"77885":-1,"78808":1,"44941":1711,"50025":1225,"91715":8,"91695":-8,"91793":-148,"91749":-5,"36041":1389,"43773":-57,"43772":3,"69434":21,"46731":3242,"69432":405,"24299":39,"68063":-107,"90958":-1,"20316":2,"21695":4,"19681":90,"19724":310,"25235":1,"44421":1,"94163":20,"96088":-88,"1577":1,"24517":8,"24531":8,"81127":28,"21675":2159,"12332":7,"19697":2070,"19698":-1265,"3146":1,"19528":-2,"48221":1,"66608":205,"19718":-2571,"19741":-3375,"19723":1959,"19725":751,"19726":2059,"19727":49,"19722":-129,"19703":-527,"12335":2,"76254":2,"12331":10,"12341":2,"12161":2,"12333":2,"45176":167,"24282":310,"100078":292,"100414":123,"100567":-1,"38030":-5,"97741":200,"24272":525,"19552":1,"27822":1,"44422":1,"24226":1,"2621":1,"32686":1,"15352":-1,"15427":-1,"100795":-33,"9287":-3,"22992":50,"12138":4,"22994":100,"24509":6,"23040":82,"26908":-2,"165":-2,"26451":-2,"28164":-2,"26442":-2,"26169":-2,"2012":-2,"28431":-2,"89140":353,"68944":12,"74328":1239,"76491":1,"44221":4,"77451":-1,"89218":-1,"19728":-5973,"92272":174,"92317":15,"24518":2,"86549":1,"24356":405,"12334":5,"21689":2,"74996":2,"71581":30,"19729":809,"67348":1,"12342":1,"75241":181,"12510":2,"24319":3,"24329":3,"24351":673,"66524":4,"39127":-1,"24511":-43,"19554":6,"66670":8,"24283":789,"67826":3,"19976":1,"44133":-9,"24274":76,"24276":334,"24278":346,"24280":1599,"24291":260,"24294":226,"24295":664,"44331":-1,"71641":226,"24305":1,"9333":-5,"24288":45,"24300":722,"19748":1118,"66637":4,"19745":-149,"89182":1,"23045":-10,"19615":-3,"67261":-1,"24279":295,"24334":3,"19721":846,"34720":1,"19925":19,"78753":2,"78758":1,"67920":1,"477":1,"1433":1,"1457":1,"1749":1,"2318":1,"26776":1,"26777":1,"26921":1,"28161":1,"285":1,"307":1,"329":1,"786":1,"849":1,"1040":1,"1156":1,"1985":1,"2765":1,"26339":1,"26357":1,"26683":1,"27303":1,"27468":1,"28381":1,"68831":1,"2110":1,"26503":1,"27783":1,"28089":1,"67464":1,"449":1,"507":1,"590":1,"936":2,"1245":1,"1490":1,"1505":1,"1557":1,"1751":1,"1760":1,"1940":1,"1949":1,"2310":1,"2317":1,"2649":1,"2656":1,"2807":1,"26432":1,"26465":1,"26595":1,"26617":1,"26915":1,"26932":1,"26938":1,"26940":1,"27099":1,"27105":1,"27230":1,"27389":1,"27556":1,"27588":1,"27712":1,"28048":1,"28355":1,"28490":1,"28499":1,"28660":1,"66902":125,"46747":54,"37091":-2,"73248":629,"12502":14,"43319":1177,"12511":6,"24318":2,"24520":2,"37907":2,"24297":323,"24322":2,"12135":2,"19560":4,"24504":2,"12506":5,"19730":-6189,"77112":48,"70438":2,"72270":-1,"24324":1,"37512":1,"37824":1,"78200":4,"9346":6,"9264":1,"19601":3,"70093":2,"28064":1,"44251":1,"47643":1,"47672":1,"82582":347,"95692":33,"96533":1,"96561":2,"96680":5,"24335":1,"95860":9,"70842":3,"22331":4,"19675":2,"93794":-1,"100858":-2,"93887":-1,"71250":-8,"24287":1750,"79786":1,"79899":-387,"88336":1,"91108":-1,"96471":6,"69822":-5,"12509":1,"19542":3,"19551":4,"36725":1,"12142":1,"19557":8,"74947":-1,"96426":-2,"24304":3,"100055":57,"101295":-1,"97291":-2,"95771":-1,"97797":-1,"78332":-1,"72315":1,"67249":-1,"24315":-9,"96083":1,"97790":1,"24336":9,"24337":5,"24338":5,"24340":6,"97901":-1,"97561":1,"12254":10,"12505":2,"12507":2,"12508":1,"12546":3,"44201":-1,"47088":1,"24357":723,"36449":-1,"76830":-1,"9293":-8,"24358":659,"1559":-1,"101651":-2,"97309":-1,"95942":-3,"24313":1,"68370":-1,"44108":-1,"44416":-1,"44231":-2,"101245":-2,"96943":2,"97690":2,"2027":1,"19534":10,"74202":1314,"77261":-6,"97358":-38,"24310":-9,"24314":1,"24289":896,"89141":3,"89103":1,"1331":-1,"19986":-1,"24696":1,"89098":1,"19540":1,"79047":1,"97102":54,"100532":1,"86269":8,"68646":8,"24508":2,"19702":-3937,"76933":-100,"77372":-12,"77398":-2,"77355":-1,"78897":-1,"77818":-1,"78999":-1,"68339":1,"36450":1,"68318":1,"24466":4,"24534":3,"27176":1,"68335":1,"68115":1,"44441":1,"97254":7,"74863":2,"37644":1,"38580":1,"78572":2,"78613":3,"78260":3,"68330":1,"36448":2,"85720":2,"38638":1,"73834":1,"79230":135,"79489":5,"24512":1,"27454":1,"95638":3,"825":1,"96978":-1,"97269":1,"95813":500,"20852":1,"96357":1,"97235":-1,"97366":-1,"68030":1,"99965":1,"100090":1,"89216":1,"100916":-1,"45177":-58,"68338":1,"64118":1,"20015":1,"24347":186,"68337":1,"19679":53,"19680":57,"19682":33,"19683":288,"19684":467,"19685":1331,"19686":518,"19687":6,"19688":133,"19704":78,"19709":621,"19712":63,"19714":28,"19735":1744,"19737":59,"19742":86,"19744":334,"19746":55,"19747":2131,"19750":-15,"19789":12,"19790":1184,"19791":33,"19792":1,"19793":2,"19794":201,"24273":496,"24275":191,"24281":1598,"24286":1495,"24290":464,"24296":449,"24298":1507,"24342":361,"24343":251,"24344":1485,"24346":323,"24348":1469,"24349":1632,"24352":357,"24353":289,"24354":1613,"24355":1740,"24363":1807,"36059":2500,"36060":2500,"36061":2500,"37897":9,"38216":1,"43320":5,"46681":780,"46682":-68,"46732":5,"46734":57,"46740":38,"46741":1,"46744":1,"46746":61,"47909":2,"48806":3,"48807":2,"49523":40,"62942":1,"69392":31,"70426":15,"70718":9,"71069":7,"71692":34,"71952":66,"71994":113,"74090":160,"74982":19,"75270":24,"75694":22,"75862":34,"76799":36,"77651":216,"83103":473,"83284":222,"83757":100,"86601":365,"86967":21,"87528":2,"89271":378,"91684":55,"91697":466,"91726":34,"91786":54,"91838":719,"77669":-30,"68332":1,"2004":1,"27610":-1,"758":-1,"1833":-1,"985":-1,"28794":-1,"12234":5,"12534":6,"91768":4,"24502":1,"12808":106,"24704":4,"24577":6,"24713":4,"24647":32,"24620":2,"24606":4,"27781":1,"24574":8,"24581":5,"24701":5,"24728":5,"24635":3,"24707":4,"27934":1,"24598":1,"24556":2,"24596":1,"24764":3,"24731":3,"24770":1,"24686":3,"24614":2,"24695":2,"24546":3,"24617":2,"24569":1,"24775":2,"24722":19,"24716":2,"24767":1,"24690":2,"24778":3,"24629":1,"24550":2,"24698":2,"24553":1,"24579":1,"24604":1,"24611":2,"24623":2,"24656":1,"24593":1,"24740":1,"24659":1,"24692":27,"24796":1,"12134":10,"12241":6,"90496":-1,"12128":-143,"12253":9,"12330":9,"68340":1,"19984":1,"20002":-1,"77604":3,"45212":-1,"74924":31,"75409":-627,"72974":70,"73402":39,"76740":40,"73705":23,"73561":36,"77172":25,"75220":32,"72657":26,"73992":24,"71509":59,"72336":26,"72468":37,"75653":72,"74268":9,"73331":32,"73478":52,"74696":56,"75096":27,"73848":37,"74100":20,"74916":46,"45208":1,"49487":-1,"49489":1,"49496":1,"49492":-2,"45199":1,"45202":1,"70960":-1,"70743":1,"73517":-1,"75316":-1,"78797":1,"23006":-100,"67836":1,"68323":-1,"64677":-1,"1497":1,"24467":2,"28262":1,"47577":-1,"97270":-1,"9349":2,"24516":2,"24330":-10,"79726":1,"79763":1,"46711":-16,"70047":1,"78116":-1,"96536":-33,"91751":4,"82534":1,"83195":1,"84044":1,"12143":5,"21156":1,"85747":1,"87557":1,"92923":1,"82866":10,"83545":2,"78462":-1,"26998":1,"1822":1,"69063":1,"64181":-1,"64187":-1,"64164":-1,"48222":-3,"68329":-1,"24500":1,"28730":1,"68645":-3,"70066":12,"70069":4,"70087":12,"70091":6,"70094":6,"70096":6,"70098":6,"70100":6,"70102":6,"70104":6,"70105":6,"70111":6,"70113":12,"70114":6,"70116":6,"70117":6,"70118":6,"70121":6,"70202":6,"70203":6,"70223":6,"95221":-6,"83703":-2,"84010":2,"95496":-2,"83258":2,"12814":2,"73062":1,"79264":18,"97055":2,"24532":2,"78474":-18,"92209":1,"850":2,"24464":4,"93938":-1,"94884":-1,"77968":-1,"20000":-49,"79031":-1,"44642":-13,"43426":-1,"43427":-2,"46762":-1,"95906":-1,"98810":-1,"96593":-2,"79633":-1,"72023":-2,"93287":-1,"75284":-2,"93727":-1,"83262":-1,"73481":-1,"93852":-1,"91143":-1,"85998":-1,"80224":-1,"86331":-1,"38794":-1,"24554":-1,"73113":39,"82416":-1,"83410":3,"25188":1,"97570":-1,"67368":-1,"21666":-1,"97832":1,"87455":-50,"13417":2,"45915":-2,"2527":-1,"68331":2,"89412":1,"37745":1,"37750":1,"74356":-1,"19529":1,"96502":-1,"95689":3,"96151":1,"2115":1,"27343":1,"86069":3,"39752":103,"27644":1,"73716":1,"78650":1,"44351":1,"77679":-4,"91270":-3,"80269":-5,"77728":-1,"77706":-3,"80623":-2,"77705":-4,"91246":-4,"85993":-5,"77751":-8,"45136":-1,"629":-1,"866":-2,"26237":-1,"25961":-1,"26708":-1,"26693":-1,"1862":-1,"1042":-1,"2400":-1,"26869":-1,"26544":-1,"2386":19,"446":5,"473":1,"1889":3,"25211":1,"28356":1,"28366":5,"34377":1,"24732":6,"82060":3,"86694":10,"36520":-9,"9300":-1,"44123":-7,"23043":-1,"25595":-1,"1443":-1,"26508":-1,"36474":-2,"21682":-1,"36583":-1,"78252":-1,"97982":-3,"41824":2,"41886":1,"88187":-1,"5027":-1,"68315":1,"43451":-1,"9331":-2,"9299":-19,"24874":1,"24871":1,"36476":-1,"44451":-1,"24465":1,"65330":-1,"68110":2,"68120":-1,"70820":3,"20323":1,"66946":-1,"24884":1,"36076":1,"64110":1,"20013":2,"20001":1,"20003":1,"43485":1,"24572":1,"97777":-1,"32141":-1,"2128":1,"6458":1,"3174":-1,"12238":1,"12246":2,"36731":-149,"46742":-1,"44640":17,"66210":2,"9277":-1,"25969":-1,"47091":-1,"80":-1,"47089":-1,"659":-1,"80545":-1,"77147":-1,"24510":1,"24472":1,"24431":2,"24427":2,"19619":1,"9280":-4,"19568":2,"97141":1,"99366":1,"37088":1,"19578":1,"24360":1,"100693":1,"100849":1,"100048":1,"100230":1,"100158":1,"100580":1,"100579":1,"100739":1,"100400":1,"100368":1,"100527":1,"100448":1,"100345":1,"100924":1,"100194":1,"100625":1,"100390":1,"100455":1,"100442":1,"100453":1,"100429":1,"100752":1,"100411":1,"100893":1,"100561":1,"100542":1,"100385":1,"100450":1,"100934":1,"100614":1,"100794":1,"100148":1,"100144":1,"100659":1,"86804":-2,"39042":1,"39018":1,"87757":10,"87645":1,"2258":1,"24301":1,"27489":1,"12535":2,"19531":2,"69953":5,"19920":-7,"46766":2,"47051":-1,"71457":-1,"85733":-25,"68321":1,"26548":1,"23197":1,"23205":1,"2062":1,"208":1,"27485":3,"27163":1,"641":1,"25988":2,"27634":2,"1209":1,"27324":1,"27963":2,"703":2,"29179":1,"27164":1,"28277":1,"1654":1,"616":2,"26220":1,"25958":1,"84120":1,"896":1,"83742":1,"23201":1,"28272":1,"23194":1,"45148":1,"91":1,"26379":2,"115":1,"2024":1,"28422":1,"2555":1,"27481":1,"2709":1,"25952":1,"26148":1,"27502":1,"47097":1,"27010":1,"26383":1,"2543":1,"23202":1,"47099":1,"28127":1,"27798":1,"27482":1,"28278":1,"47096":1,"2397":1,"83751":1,"1349":1,"714":1,"251":1,"27498":1,"27006":1,"27338":1,"26703":1,"26849":1,"83736":1,"23183":1,"1640":1,"47086":1,"1047":1,"45152":1,"203":1,"27174":1,"26709":1,"25975":1,"1192":1,"83075":1,"2127":1,"680":1,"82":1,"27321":1,"26138":2,"82960":1,"47085":1,"47093":1,"27477":1,"1650":1,"188":1,"886":1,"28265":2,"1861":1,"27794":1,"26227":1,"24547":159,"24549":168,"24558":250,"24619":250,"24693":250,"24706":92,"24763":229,"24777":241,"24552":148,"24576":160,"24610":250,"24616":212,"24622":153,"24625":250,"24628":250,"24657":137,"24660":172,"24705":250,"24730":250,"24631":192,"24613":229,"24733":107,"24786":250,"24727":184,"24646":250,"24736":154,"24595":170,"24697":192,"24721":250,"24724":143,"24573":158,"24626":15,"24633":10,"24700":216,"24766":177,"24603":250,"24795":90,"24689":127,"24712":85,"12336":1,"48882":1,"12236":1,"70693":1,"45001":1,"45003":1,"66943":1,"66952":1,"36038":1,"26114":1,"12351":-152,"12543":-152,"12155":48,"12156":-152,"91690":152,"68373":-21,"68376":-8,"93269":1,"9377":2,"19530":2,"424":1,"32591":1},"curr":{"3":-1874,"1":21627405,"62":136,"15":1024,"23":-99,"2":209366,"65":58,"26":58,"7":335,"69":-657},"mf":0,"TimeStamp":"0001-01-01T00:00:00"}}}',
  // 1k drop
  // // '{"Kind":"data","Payload":{"Character":"1","Drop":{"items":{"24272":1,"24278":1,"24284":1,"24508":4,"46735":85,"96464":3,"96722":5,"24300":2,"84731":11,"24473":2,"73248":23,"19699":3,"19702":3,"19718":2,"85016":-23,"21675":-2,"74996":-1,"64736":-2,"24509":1,"19700":99,"24475":5,"19722":58,"45175":80,"89140":146,"19721":-3721,"89103":64,"68335":-1,"45178":1,"36449":2,"19575":-10,"71581":3,"81479":-1,"8920":-24,"21683":2,"95933":-5,"99004":-8,"44205":-7,"74202":5,"24299":16,"83103":5,"19748":80,"46731":153,"36448":-27,"96144":1,"24277":36587,"44081":-2,"24288":2,"19701":9,"24276":18,"24304":1,"49424":63,"75919":-11,"78260":-3,"78572":-3,"78200":-2,"78613":-3,"43773":-39,"24353":1,"24297":1,"87701":1,"69432":2,"83008":7,"44231":-1,"81127":6,"19686":4,"24517":4,"92272":144,"92317":10,"19687":1,"24511":2,"24523":-2,"24531":1,"24476":1,"45176":19,"19727":4,"24516":1,"24510":2,"46733":99,"95982":-1,"19729":32,"65388":2,"19728":1,"19724":4,"24518":1,"22997":-50,"38030":1,"97291":2,"24294":14,"24341":5,"9293":-1,"19608":-1,"97570":2,"22331":2,"86181":2,"24356":15,"24358":1,"24360":-1,"754":1,"27439":1,"19725":4,"19745":11,"19732":11,"45177":2,"96561":10,"23038":9,"24884":1,"20323":1,"19723":4,"19739":2,"19741":8,"19743":3,"19925":3,"24274":3,"24273":11,"24282":11,"19620":4,"12128":1,"12134":-2,"12142":4,"12342":-1,"24350":9,"12135":12,"12334":12,"12335":5,"12533":5,"24842":1,"75284":-1,"43319":13,"24520":1,"19730":4,"19731":5,"43766":5,"82582":10,"77256":3,"43772":3,"24351":1,"94163":1,"70842":1,"74528":1,"26559":1,"68326":1,"68646":16,"24295":2,"1302":1,"24467":1,"24534":1,"68339":1,"82632":1,"68316":1,"19726":13,"19682":3,"19683":1,"12248":5,"19553":3,"28768":1,"36471":2,"44376":3,"26554":1,"27354":1,"27811":1,"36520":3,"44424":1,"281":1,"28074":1,"1394":1,"2229":1,"36681":1,"96800":1,"37675":1,"38935":1,"91689":-1,"12156":-1,"12137":-2,"12178":-2,"12267":-1,"12268":1,"12271":-1,"12275":1,"12328":-2,"12138":-1,"12329":-1,"68314":-35,"28097":1,"19697":8,"64672":-1,"19719":2,"66345":-1,"64670":-1,"27508":-1,"26726":-1,"44941":20,"50025":3,"36041":37,"92072":2,"44196":1,"66608":11,"66670":1,"77604":1,"69434":7,"69392":9,"32132":-1,"6472":-1,"6470":-1,"6549":-1,"6473":-1,"33345":-1,"95434":1,"95445":1,"95420":1,"95399":1,"95323":1,"95360":1,"95370":1,"72558":-1,"46738":2,"46076":-1,"24466":1,"27966":1,"73539":1,"12544":1,"74090":2,"12333":2,"12545":1,"12144":1,"12510":2,"86798":12,"86843":12,"19570":2,"19703":3,"19698":3,"24507":1,"24875":1,"24872":1,"66637":1,"9566":-4,"67836":1,"68323":-1,"19983":1,"68341":-1,"19679":-9,"19710":-6,"95797":3,"97772":3,"97857":1,"95767":1,"96839":3,"97507":1,"100975":1,"96151":1,"97358":2,"100654":5,"95638":-1,"96638":-1,"97269":-1,"616":-1,"27638":-1,"19529":6,"28850":1,"87517":1,"24532":1,"19737":-20,"88866":1,"75851":10,"99293":4,"118":1,"95986":1,"96978":1,"97132":1,"24568":-220,"24697":-312,"24705":-402,"24721":-498,"24727":-255,"24730":-333,"24736":-256,"24777":-364,"24795":-118,"89098":30,"89216":34,"89258":41,"24557":-242,"24595":-256,"24685":-217,"24693":-342,"24694":-421,"24700":-313,"24712":-133,"24724":-209,"24763":-366,"24766":-256,"24769":-397,"24774":-275,"24786":-428,"24549":-204,"24634":-211,"24689":-165,"24706":-126,"24709":-226,"24715":-120,"24733":-111,"24739":-231,"97254":1,"97185":1,"97518":1,"97797":1,"97894":1,"96003":1,"28726":1,"68638":248,"94653":-231,"68635":4,"68632":3,"68637":3,"68634":1,"68636":3,"68633":3,"68387":-1,"101292":-1,"54602":1,"63642":1,"64531":1,"44201":-1,"65396":-1,"9285":-1,"24320":1,"65634":-2,"44441":-1,"46739":1,"24289":1,"25215":-1,"21689":-4,"29156":-1,"37334":-1,"37591":-1,"38656":-1,"37377":-1,"38609":-1,"43556":-2,"37833":-1,"39027":-1,"37354":-1,"37589":-1,"38549":-1,"38744":-1,"38579":-1,"38594":-1,"49426":3,"24329":1,"44401":-2,"65636":-2,"24335":2,"65638":-2,"65637":-4,"9349":-12,"24319":1,"44133":-1,"44210":-1,"9277":-1,"44138":-1,"79489":-4,"79909":-3,"19542":-4,"39601":-1,"100153":-1,"100947":-1,"38867":-1,"100849":-1,"100432":-1,"100074":-1,"26703":-1,"27324":-1,"79851":-3,"21157":-1,"12236":4,"91699":1,"87757":5},"curr":{"1":-109686,"2":52969,"3":56,"62":20,"15":-1062,"18":4,"36":-30,"20":25,"23":7},"mf":0,"TimeStamp":"0001-01-01T00:00:00"}}}',
  // gw2sharp bug test (unknown + known item and currency)
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"78599":1,"70093":1},"curr":{"1":10000,"2":1,"80":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"78599":-1,"70093":-1},"curr":{"1":-10000,"2":-1,"80":-1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // all rarities name test
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19620":1,"96144":1,"85016":1,"84731":1,"83008":1,"87557":1,"46740":2,"19925":-1},"curr":{"1":123456, "2":-3},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // tooltip test: different profits
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"78599":1,"70093":1,"20316":1,"9353":1,"19718":2,"2392":-1,"923":1,"142":2,"2388":2,"19703":1,"2384":1,"19620":1,"19529":2,"85016":1,"84731":2,"83008":-10,"46740":1},"curr":{"1":123456, "2":-3, "80":5},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // profit test: same item, different counts
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"84731":1},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"84731":2},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // count: small/large, positive/negative
  // '{"kind":"data","payload":{"character":"1","drop":{"items":{"12538":1,"12537":-1,"12255":123, "12536":-123, "12248":45678, "12243":-45678},"curr":{},"mf":312,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  // coin: none, only copper, only silver, only gold, mixed, single digit, double digit, gold 3 digit
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":-9123456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":9123456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":123456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":123400},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":120056},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":120000},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":20406},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // single (gold, silver, copper)
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":20006},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // single (gold, copper)
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":20000},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // single gold
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":3400},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":3456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":406},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // single (silver, copper)
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":400},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // single silver
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":56},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":6},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // single copper
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"2":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // no coin

  // coin: above max coin profit
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":123456789012345},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  // max coin profit
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":10000000000},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  // all types: custom coin, api miss currency+item, api has currency+items
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19925":12, "78599":6},"curr":{"1":123456,"64":8, "80":5},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  // items + currencies available in api
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19925":1},"curr":{"64":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',

  // api miss
  // api miss item 78599 (lvl 80 boost)
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"78599":3},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // api miss currency
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{},"curr":{"80":5},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"5","drop":{"items":{},"curr":{"17":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}'
  // api miss item  + currency
  // '{"kind":"data","payload":{"character":"4","drop":{"items":{"95195":1},"curr":{"17":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
]

async function wait(waitTimeMs) {
  return new Promise(r => setTimeout(r, waitTimeMs));
}

function closeConnection() {
  console.log("isAuthenticated NO");
  webSocketServer.clients.forEach(c => c.close(1007, "no valid session provided"));
}

/* https://www.rfc-editor.org/rfc/rfc6455.html#section-7.4
   1000

      1000 indicates a normal closure, meaning that the purpose for
      which the connection was established has been fulfilled.

   1001

      1001 indicates that an endpoint is "going away", such as a server
      going down or a browser having navigated away from a page.

   1002

      1002 indicates that an endpoint is terminating the connection due
      to a protocol error.

   1003

      1003 indicates that an endpoint is terminating the connection
      because it has received a type of data it cannot accept (e.g., an
      endpoint that understands only text data MAY send this if it
      receives a binary message).

   1004

      Reserved.  The specific meaning might be defined in the future.

   1005
      1005 is a reserved value and MUST NOT be set as a status code in a
      Close control frame by an endpoint.  It is designated for use in
      applications expecting a status code to indicate that no status
      code was actually present.

   1006
      1006 is a reserved value and MUST NOT be set as a status code in a
      Close control frame by an endpoint.  It is designated for use in
      applications expecting a status code to indicate that the
      connection was closed abnormally, e.g., without sending or
      receiving a Close control frame.

   1007
      1007 indicates that an endpoint is terminating the connection
      because it has received data within a message that was not
      consistent with the type of the message (e.g., non-UTF-8 [RFC3629]
      data within a text message).

   1008
      1008 indicates that an endpoint is terminating the connection
      because it has received a message that violates its policy.  This
      is a generic status code that can be returned when there is no
      other more suitable status code (e.g., 1003 or 1009) or if there
      is a need to hide specific details about the policy.

   1009
      1009 indicates that an endpoint is terminating the connection
      because it has received a message that is too big for it to
      process.

   1010
      1010 indicates that an endpoint (client) is terminating the
      connection because it has expected the server to negotiate one or
      more extension, but the server didn't return them in the response
      message of the WebSocket handshake.  The list of extensions that
*/
