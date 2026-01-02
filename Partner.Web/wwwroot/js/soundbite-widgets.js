(function () { 

  /////[ Private Variables ]////////////////////////////////////////////////////////////////////////

  self = {};                // Protected reference to 'this'
  index = 0;                // Stores incremental value used for ID generation
  complete = {}             // Stores dictionary of request IDs and completion status
  isHostListening = false;

  /////[ Public Properties ]////////////////////////////////////////////////////////////////////////


  /**
   * Specifies the target origin of the recipient window.  By default this value is "*" meaning it
   * will be received by any window regardless of the URI.  A URI can be specified for additional
   * security if so desired.
   **/  
  self.targetOrigin = "*";


  function ensureHostListening() {
    if (!isHostListening) {
      isHostListening = true;
      window.addEventListener('message', function (msg) {
        if (complete[msg.data] === false) {
          complete[msg.data] = true;
        }
      });
    }
  }

  function waitForWidgetResponse(widgetWindow, id) {
    setTimeout(function () {
      if (!complete[id]) {
        widgetWindow.postMessage({ id: id }, "*");
        waitForWidgetResponse(widgetWindow, id);
      } else {
        delete complete[id];
        alert('done');  
      }
    }, 100);
  }

  self.configureWidget = function (iFrameId, configuration) {
    ensureHostListening();
    var widgetWindow = document.getElementById(iFrameId).contentWindow;
    var id = 'Widget' + (index++);
    complete[id] = false;
    waitForWidgetResponse(widgetWindow, id);
  }

  self.getWidgetConfig = function () { 
    window.addEventListener('message', function (msg) {
      msg.source.postMessage(msg.data.id, "*");
    });
  }

  window.soundbite = self;

})()

