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
  // 1k drop
  // '{"Kind":"data","Payload":{"Character":"1","Drop":{"items":{"24272":1,"24278":1,"24284":1,"24508":4,"46735":85,"96464":3,"96722":5,"24300":2,"84731":11,"24473":2,"73248":23,"19699":3,"19702":3,"19718":2,"85016":-23,"21675":-2,"74996":-1,"64736":-2,"24509":1,"19700":99,"24475":5,"19722":58,"45175":80,"89140":146,"19721":-3721,"89103":64,"68335":-1,"45178":1,"36449":2,"19575":-10,"71581":3,"81479":-1,"8920":-24,"21683":2,"95933":-5,"99004":-8,"44205":-7,"74202":5,"24299":16,"83103":5,"19748":80,"46731":153,"36448":-27,"96144":1,"24277":36587,"44081":-2,"24288":2,"19701":9,"24276":18,"24304":1,"49424":63,"75919":-11,"78260":-3,"78572":-3,"78200":-2,"78613":-3,"43773":-39,"24353":1,"24297":1,"87701":1,"69432":2,"83008":7,"44231":-1,"81127":6,"19686":4,"24517":4,"92272":144,"92317":10,"19687":1,"24511":2,"24523":-2,"24531":1,"24476":1,"45176":19,"19727":4,"24516":1,"24510":2,"46733":99,"95982":-1,"19729":32,"65388":2,"19728":1,"19724":4,"24518":1,"22997":-50,"38030":1,"97291":2,"24294":14,"24341":5,"9293":-1,"19608":-1,"97570":2,"22331":2,"86181":2,"24356":15,"24358":1,"24360":-1,"754":1,"27439":1,"19725":4,"19745":11,"19732":11,"45177":2,"96561":10,"23038":9,"24884":1,"20323":1,"19723":4,"19739":2,"19741":8,"19743":3,"19925":3,"24274":3,"24273":11,"24282":11,"19620":4,"12128":1,"12134":-2,"12142":4,"12342":-1,"24350":9,"12135":12,"12334":12,"12335":5,"12533":5,"24842":1,"75284":-1,"43319":13,"24520":1,"19730":4,"19731":5,"43766":5,"82582":10,"77256":3,"43772":3,"24351":1,"94163":1,"70842":1,"74528":1,"26559":1,"68326":1,"68646":16,"24295":2,"1302":1,"24467":1,"24534":1,"68339":1,"82632":1,"68316":1,"19726":13,"19682":3,"19683":1,"12248":5,"19553":3,"28768":1,"36471":2,"44376":3,"26554":1,"27354":1,"27811":1,"36520":3,"44424":1,"281":1,"28074":1,"1394":1,"2229":1,"36681":1,"96800":1,"37675":1,"38935":1,"91689":-1,"12156":-1,"12137":-2,"12178":-2,"12267":-1,"12268":1,"12271":-1,"12275":1,"12328":-2,"12138":-1,"12329":-1,"68314":-35,"28097":1,"19697":8,"64672":-1,"19719":2,"66345":-1,"64670":-1,"27508":-1,"26726":-1,"44941":20,"50025":3,"36041":37,"92072":2,"44196":1,"66608":11,"66670":1,"77604":1,"69434":7,"69392":9,"32132":-1,"6472":-1,"6470":-1,"6549":-1,"6473":-1,"33345":-1,"95434":1,"95445":1,"95420":1,"95399":1,"95323":1,"95360":1,"95370":1,"72558":-1,"46738":2,"46076":-1,"24466":1,"27966":1,"73539":1,"12544":1,"74090":2,"12333":2,"12545":1,"12144":1,"12510":2,"86798":12,"86843":12,"19570":2,"19703":3,"19698":3,"24507":1,"24875":1,"24872":1,"66637":1,"9566":-4,"67836":1,"68323":-1,"19983":1,"68341":-1,"19679":-9,"19710":-6,"95797":3,"97772":3,"97857":1,"95767":1,"96839":3,"97507":1,"100975":1,"96151":1,"97358":2,"100654":5,"95638":-1,"96638":-1,"97269":-1,"616":-1,"27638":-1,"19529":6,"28850":1,"87517":1,"24532":1,"19737":-20,"88866":1,"75851":10,"99293":4,"118":1,"95986":1,"96978":1,"97132":1,"24568":-220,"24697":-312,"24705":-402,"24721":-498,"24727":-255,"24730":-333,"24736":-256,"24777":-364,"24795":-118,"89098":30,"89216":34,"89258":41,"24557":-242,"24595":-256,"24685":-217,"24693":-342,"24694":-421,"24700":-313,"24712":-133,"24724":-209,"24763":-366,"24766":-256,"24769":-397,"24774":-275,"24786":-428,"24549":-204,"24634":-211,"24689":-165,"24706":-126,"24709":-226,"24715":-120,"24733":-111,"24739":-231,"97254":1,"97185":1,"97518":1,"97797":1,"97894":1,"96003":1,"28726":1,"68638":248,"94653":-231,"68635":4,"68632":3,"68637":3,"68634":1,"68636":3,"68633":3,"68387":-1,"101292":-1,"54602":1,"63642":1,"64531":1,"44201":-1,"65396":-1,"9285":-1,"24320":1,"65634":-2,"44441":-1,"46739":1,"24289":1,"25215":-1,"21689":-4,"29156":-1,"37334":-1,"37591":-1,"38656":-1,"37377":-1,"38609":-1,"43556":-2,"37833":-1,"39027":-1,"37354":-1,"37589":-1,"38549":-1,"38744":-1,"38579":-1,"38594":-1,"49426":3,"24329":1,"44401":-2,"65636":-2,"24335":2,"65638":-2,"65637":-4,"9349":-12,"24319":1,"44133":-1,"44210":-1,"9277":-1,"44138":-1,"79489":-4,"79909":-3,"19542":-4,"39601":-1,"100153":-1,"100947":-1,"38867":-1,"100849":-1,"100432":-1,"100074":-1,"26703":-1,"27324":-1,"79851":-3,"21157":-1,"12236":4,"91699":1,"87757":5},"curr":{"1":-109686,"2":52969,"3":56,"62":20,"15":-1062,"18":4,"36":-30,"20":25,"23":7},"mf":0,"TimeStamp":"0001-01-01T00:00:00"}}}',
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
  '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":-9123456},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
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
  '{"kind":"data","payload":{"character":"3","drop":{"items":{"19925":1},"curr":{"64":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',

  // api miss
  // api miss item 78599 (lvl 80 boost)
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"78599":3},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // api miss currency
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{},"curr":{"80":5},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"5","drop":{"items":{},"curr":{"17":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}'
  // api miss item  + currency
  '{"kind":"data","payload":{"character":"4","drop":{"items":{"95195":1},"curr":{"17":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
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
