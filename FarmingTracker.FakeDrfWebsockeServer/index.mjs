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
  // all rarities name test
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19620":1,"96144":1,"85016":1,"84731":1,"83008":1,"87557":1,"46740":2,"19925":-1},"curr":{"1":123456, "2":-3},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // tooltip test
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"78599": 1, "19620":1,"96144":1,"85016":1,"84731":1,"83008":-2,"87557":2,"46740":1,"19925":-1},"curr":{"1":123456, "2":-3, "80":5},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // profit test: same item, different counts
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"84731":1},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"84731":2},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // count: small/large, positive/negative
  // '{"kind":"data","payload":{"character":"1","drop":{"items":{"12538":1,"12537":-1,"12255":123, "12536":-123, "12248":45678, "12243":-45678},"curr":{},"mf":312,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  
  // coin: none, only copper, only silver, only gold, mixed, single digit, double digit, gold 3 digit
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
  '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":56},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":6},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // single copper
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"2":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}', // no coin

  // coin: above max coin profit
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":123456789012345},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  // max coin profit
  // '{"kind":"data","payload":{"character":"2","drop":{"items":{},"curr":{"1":10000000000},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  // all types: custom coin, api miss currency+item, api has currency+items
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19925":12, "78599":6},"curr":{"1":123456,"64":8, "80":5},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  
  // items + currencies available in api
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"19925":12},"curr":{"64":8},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',

  // api miss
  // api miss item 78599 (lvl 80 boost)
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{"78599":3},"curr":{},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // api miss currency 80
  // '{"kind":"data","payload":{"character":"3","drop":{"items":{},"curr":{"80":5},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // api miss item 95195, currency 17
  // '{"kind":"data","payload":{"character":"4","drop":{"items":{"95195":5},"curr":{"17":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}',
  // api miss currency 17
  // '{"kind":"data","payload":{"character":"5","drop":{"items":{},"curr":{"17":1},"mf":0,"timestamp":"2022-12-09T05:17:36.745Z"}}}'
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
