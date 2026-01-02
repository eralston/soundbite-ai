import { InitFeedWidget } from "./models/InitFeedWidget";
import { InitPlayerWidget } from "./models/InitPlayerWidget";
import { InitSessionWidget } from "./models/InitSessionWidget";
import { InitTeamsWidget } from "./models/InitTeamsWidget";
import { WidgetMessage } from "./models/WidgetMessage";

class WidgetManagerClass {
  /////[ Fields ]///////////////////////////////////////////////////////////////////////////////////

  private window: Window; // Stores a reference to the window that is listening for messages
  private complete: any = Object.create(null); // Stores dictionary of request IDs and completion status
  private requestIndex: number = 0; // Stores incremental value used created unique request IDs

  /////[ Properties ]///////////////////////////////////////////////////////////////////////////////

  /**
   * Specifies the target origin of the recipient window.  By default this value is "*" meaning it
   * will be received by any window regardless of the URI.  A URI can be specified for additional
   * security if so desired.
   **/
  targetOrigin: string = "*";

  /**
   * Specifies the URL of the hosting location of the widgets.  This value should end with a slash.
   */
  widgetUrl: string = "https://www.soundbite.ai/widgets/";

  onFeedTestButtonClicked: ((data: any) => void) | null = null;

  /////[ Constructor ]//////////////////////////////////////////////////////////////////////////////

  /**
   * Creates a new WidgetManagerClass instance
   * @param window - reference to the window that will listen for events
   */
  constructor(window: Window) {
    this.window = window;
    const self = this;

    const handler = (event: MessageEvent) => {
      var message = event.data as WidgetMessage;
      if (message.messageId && message.message) {
        switch (message.message) {
          case "sb_initialized":
            if (self.complete[message.messageData] === false) {
              self.complete[message.messageData] = true;
            }
            break;
          case "TestButtonClicked":
            if (this.onFeedTestButtonClicked !== null)
              this.onFeedTestButtonClicked(message.messageData);
            break;
        }
      }
    };

    // The following listener is reponsible for processing incomming messages from widgets
    window.addEventListener("message", handler);
  }

  /////[ Methods ]/////////////////////////////////////////////////////////////////////////////////

  private getMessageId(): number {
    this.requestIndex++;
    return this.requestIndex;
  }

  private waitForWidgetInitialize(
    iFrameId: string,
    messageId: number,
    widgetName: string,
    widgetData: any
  ): Promise<void> {
    const self = this;
    const iFrame: HTMLIFrameElement = this.window.document.getElementById(
      iFrameId
    ) as HTMLIFrameElement;
    const targetWindow = iFrame?.contentWindow;

    if (!iFrame) {
      throw `Failed to acquire the iFrame by id '${iFrameId}'`;
    }

    if (!targetWindow) {
      throw `IFrame acquired by id '${iFrameId}' but content window was not available`;
    }

    self.complete[messageId] = false;

    const wait = (resolve: any, reject: any) => {
      targetWindow.postMessage(
        new WidgetMessage(widgetName, "sb_widget_init", widgetData),
        self.targetOrigin
      );
      setTimeout(() => {
        if (self.complete[messageId] === true) {
          delete self.complete[messageId];
          resolve();
        } else {
          wait(resolve, reject);
        }
      }, 100);
    };

    const promise = new Promise<void>((resolve, reject) => {
      wait(resolve, reject);
    });

    return promise;
  }

  showFeedWidget(iFrameId: string, widgetData: InitFeedWidget): Promise<void> {
    let messageId = this.getMessageId();
    return this.waitForWidgetInitialize(
      iFrameId,
      messageId,
      "Feed",
      widgetData
    );
  }

  showPlayerWidget(
    iFrameId: string,
    widgetData: InitPlayerWidget
  ): Promise<void> {
    let messageId = this.getMessageId();
    return this.waitForWidgetInitialize(
      iFrameId,
      messageId,
      "Player",
      widgetData
    );
  }

  showSessionWidget(
    iFrameId: string,
    widgetData: InitSessionWidget
  ): Promise<void> {
    let messageId = this.getMessageId();
    return this.waitForWidgetInitialize(
      iFrameId,
      messageId,
      "Session",
      widgetData
    );
  }

  showTeamsWidget(
    iFrameId: string,
    widgetData: InitTeamsWidget
  ): Promise<void> {
    let messageId = this.getMessageId();
    return this.waitForWidgetInitialize(
      iFrameId,
      messageId,
      "Teams",
      widgetData
    );
  }
}

export const WidgetManager = new WidgetManagerClass(window);
