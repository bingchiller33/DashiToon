import { toast } from "sonner";
import MessageService from "./MessageService";

const ToastService = {
    showToast: (messageCd: string, params: string[]) => {
        const message = MessageService.getMessage(messageCd);
        const content = MessageService.replaceParam(message.content, params);
        switch (message.type) {
            case "I":
                toast.info(content);
                break;
            case "W":
                toast.warning(content);
                break;
            case "S":
                toast.success(content);
                break;
            case "E":
                toast.error(content);
                break;
            default:
                toast.error(content);
                break;
        }
    },
};

export default ToastService;
