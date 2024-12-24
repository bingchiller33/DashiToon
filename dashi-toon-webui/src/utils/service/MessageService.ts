import { Message } from "@/global.model";
import messages from "@/message.json";

const MessageService = {
  get: (messageCd: string, params: string[]) => {
    const message = MessageService.getMessage(messageCd);
    const content = MessageService.replaceParam(message.content, params);
    return content;
  },

  getMessage: (messageCd: string) => {
    return (
      messages.find((item) => item.messageCd === messageCd) || {
        messageCd: "8001",
        type: "SE",
        content: `Message ${messageCd} not found`,
      }
    );
  },

  replaceParam: (content: string, params: string[]) => {
    if (params.length === 0) {
      return content;
    } else {
      for (let i = 0; i < params.length; i++) {
        content = content.replace("{" + i + "}", params[i]);
      }
      return content;
    }
  },
};

export default MessageService;
