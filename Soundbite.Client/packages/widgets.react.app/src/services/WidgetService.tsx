import { WidgetMessage } from "@soundbite/widgets-api";
import { AppStore } from "./AppStore";

class WidgetServiceClass {
  targetOrigin: string = "*";

  /**
   * Responsible for managing the incomming browser window messages from a partner web site
   * @param event - message event information
   */
  onWindowMessage(event: MessageEvent): void {
    // Acquire a reference to the parent window so it can be used to send response messages back.
    if (AppStore.parentWindow === null) {
      AppStore.parentWindow = event.source as Window;
    }

    // Acquire message data
    const message = event.data as WidgetMessage;

    if (message.messageId && message.message) {
      switch (message.message) {
        case "sb_widget_init":
          return this.onInit(message);
      }
    }
  }

  /**
   * Responsible for initializing a requested widget and sending back a message indicating the
   * initialization message was received and processed.
   * @param message - widget
   */
  onInit(message: WidgetMessage): void {
    AppStore.setCurrentWidget(message.widgetType, message.messageData);
    this.sendMessage(message.widgetType, "sb_initialized", message.messageId);
  }

  /**
   * Responsible for sending a message back to the parent window
   * @param widgetType - identifies the type of widget sending the message
   * @param messageType - identifies the message type being sent
   * @param messageData - data object conatining detailed message data
   */
  sendMessage(widgetType: string, messageType: string, messageData: any): void {
    if (AppStore.parentWindow) {
      AppStore.parentWindow.postMessage(
        new WidgetMessage(widgetType, messageType, messageData),
        this.targetOrigin
      );
    }
  }
}

export const WidgetService = new WidgetServiceClass();
