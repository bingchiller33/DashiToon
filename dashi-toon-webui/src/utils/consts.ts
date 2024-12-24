import {
    BookOpen,
    CheckCircle2,
    Edit,
    PauseCircle,
    Trash2,
} from "lucide-react";

export const PAYMENT_STATUS = {
    0: {
        label: "Đang chờ",
    },
    1: {
        label: "Thành công",
    },
    2: {
        label: "Thất bại",
    },
} as Record<number, { label: string }>;

export const SUBSCRYPTION_STATUS = {
    1: {
        content: "Đang Chờ",
        bdColor: "red-500",
        bgColor: "red-700",
    },
    2: {
        content: "Đang Hoạt Động",
        bdColor: "green-500",
        bgColor: "green-600",
    },
    3: {
        content: "???",
        bdColor: "red-500",
        bgColor: "red-700",
    },
    4: {
        content: "Đã Hủy",
        bdColor: "red-500",
        bgColor: "red-700",
    },
    5: {
        content: "???",
        bdColor: "red-500",
        bgColor: "red-700",
    },
} as Record<number, { content: string; bdColor: string; bgColor: string }>;

export const SERIES_STATUS = [
    {
        value: 1,
        content: "Tiểu thuyết",
    },
    {
        value: 2,
        content: "Truyện tranh",
    },
];

export const SERIES_STATUS2 = {
    1: {
        label: "Tiểu thuyết",
        slug: "novel",
    },
    2: {
        label: "Truyện tranh",
        slug: "comic",
    },
} as Record<number, { label: string; slug: string }>;

export const COMMISSION_TYPE = {
    0: "DashiFan",
    1: "Kana",
};

export type StatusType =
    | "Bản Nháp"
    | "Đang Ra"
    | "Tạm Dừng"
    | "Hoàn Thành"
    | "Đã Xóa";

export const STATUS_CONFIG = {
    "Bản Nháp": { color: "bg-gray-500/10 text-gray-500", icon: Edit },
    "Đang Tiến Hành": {
        color: "bg-green-500/10 text-green-500",
        icon: BookOpen,
    },
    "Tạm Dừng": {
        color: "bg-yellow-500/10 text-yellow-500",
        icon: PauseCircle,
    },
    "Hoàn Thành": { color: "bg-blue-500/10 text-blue-500", icon: CheckCircle2 },
    "Đã Xóa": { color: "bg-red-500/10 text-red-500", icon: Trash2 },
} as const;

export const RATING_CONFIG = {
    "Tất cả độ tuổi": { color: "bg-green-500/10 text-green-500" },
    "Thiếu niên": { color: "bg-blue-500/10 text-blue-500" },
    "Thanh niên": { color: "bg-purple-500/10 text-purple-500" },
    "Trưởng thành": { color: "bg-red-500/10 text-red-500" },
} as const;

export const RATING_CONFIG_2 = {
    1: {
        label: "Tất cả độ tuổi",
        color: "bg-green-500/10 text-green-500",
    },
    2: {
        label: "Thiếu niên",
        color: "bg-blue-500/10 text-blue-500",
    },
    3: {
        label: "Thanh niên",
        color: "bg-purple-500/10 text-purple-500",
    },
    4: {
        label: "Trưởng thành",
        color: "bg-red-500/10 text-red-500",
    },
} as Record<number, { label: string; color: string }>;

export const STATUS_CONFIG_2 = {
    1: {
        label: "Đang tiến hành",
        color: "bg-green-500 text-green-500",
        icon: BookOpen,
    },
    2: {
        label: "Tạm dừng",
        color: "bg-orange-500 text-orange-500",
        icon: PauseCircle,
    },
    3: {
        label: "Hoàn thành",
        color: "bg-blue-500 text-blue-500",
        icon: CheckCircle2,
    },
    4: {
        label: "Đã xoá",
        color: "bg-red-500 text-red-500",
        icon: PauseCircle,
    },
} as Record<number, { label: string; color: string; icon: any }>;

export const contentCategories = [
    {
        id: "Violent",
        label: "Nội dung bạo lực và đồ họa",
        options: [
            { value: 0, label: "Không có bạo lực, máu hoặc kinh dị" },
            { value: 1, label: "Máu nhẹ hoặc viễn tưởng trong một vài tập" },
            {
                value: 2,
                label: "Chủ đề bạo lực với máu hoặc kinh dị ở mức trung bình",
            },
            { value: 3, label: "Chi tiết về bạo lực, máu hoặc kinh dị" },
        ],
    },
    {
        id: "Nudity",
        label: "Nội dung khỏa thân",
        options: [
            { value: 0, label: "Không có khỏa thân (phần hoặc toàn bộ)" },
            {
                value: 1,
                label: "Một số nhân vật mặc trang phục tối thiểu (ví dụ: đồ bơi, đồ lót), không có chủ đề tình dục",
            },
            {
                value: 2,
                label: "Khỏa thân hài hước với việc che giấu chiến lược",
            },
            {
                value: 3,
                label: "Các cảnh 'fan-service' (tức là trang phục tối thiểu ở tư thế gợi cảm), chủ đề gợi tình",
            },
        ],
    },
    {
        id: "Sexual",
        label: "Nội dung tình dục",
        options: [
            { value: 0, label: "Không có nội dung hoặc chủ đề tình dục" },
            { value: 1, label: "Chủ đề tình dục nhẹ nhàng" },
            {
                value: 2,
                label: "Nội dung tình dục hoặc ám chỉ trong một vài tập",
            },
            {
                value: 3,
                label: "Nội dung tình dục và chủ đề tình dục xuyên suốt bộ truyện",
            },
        ],
    },
    {
        id: "Profanity",
        label: "Ngôn ngữ tục tĩu",
        options: [
            { value: 0, label: "Không có ngôn ngữ tục tĩu" },
            {
                value: 1,
                label: "Ngôn ngữ tục tĩu bị kiểm duyệt hoàn toàn (ví dụ: #$%^ trong một vài tập)",
            },
            {
                value: 2,
                label: "Ngôn ngữ tục tĩu không bị kiểm duyệt hoặc kiểm duyệt một phần trong một vài tập",
            },
            {
                value: 3,
                label: "Ngôn ngữ tục tĩu không bị kiểm duyệt xuyên suốt bộ truyện",
            },
        ],
    },
    {
        id: "Alcohol",
        label: "Rượu, thuốc lá hoặc ma túy",
        options: [
            { value: 0, label: "Không có rượu, thuốc lá hoặc ma túy" },
            {
                value: 1,
                label: "Một vài lần nhắc đến rượu, thuốc lá hoặc ma túy",
            },
            {
                value: 2,
                label: "Tiêu thụ rượu, thuốc lá hoặc ma túy ở mức ngụ ý hoặc nhẹ nhàng",
            },
            {
                value: 3,
                label: "Mô tả việc tiêu thụ rượu, thuốc lá hoặc ma túy ở mức độ trung bình đến cao",
            },
        ],
    },
    {
        id: "Sensitive",
        label: "Chủ đề và nội dung nhạy cảm",
        options: [
            { value: 0, label: "Không có chủ đề hoặc nội dung nhạy cảm" },
            {
                value: 1,
                label: "Một vài lần nhắc đến các chủ đề hoặc nội dung như tự hại, bắt nạt hoặc lạm dụng",
            },
            {
                value: 2,
                label: "Chủ đề hoặc nội dung nhạy cảm như tự hại, bắt nạt hoặc lạm dụng được khám phá nhẹ nhàng trong một số phần của câu chuyện",
            },
            {
                value: 3,
                label: "Chủ đề nhạy cảm như tự hại, bắt nạt hoặc lạm dụng được khám phá và xuất hiện liên tục xuyên suốt bộ truyện",
            },
        ],
    },
];

export const STATUS_MAP = {
    0: "Draft",
    1: "Ongoing",
    2: "Hiatus",
    3: "Complete",
    4: "Trashed",
} as const;

export const STATUS_DISPLAY_MAP = {
    Draft: "Nháp",
    Ongoing: "Đang tiến hành",
    Hiatus: "Tạm ngưng",
    Complete: "Hoàn thành",
    Trashed: "Đã xóa",
} as const;

export const STATUS_COLOR_MAP = {
    0: "bg-gray-500",
    1: "bg-green-500",
    2: "bg-yellow-500",
    3: "bg-blue-500",
    4: "bg-red-500",
} as const;

export const currencyMap = {
    1: "Kana Gold",
    2: "Kana Coin",
};

export const typeMap = {
    1: "Điểm danh",
    2: "Nhiệm vụ",
    3: "Nạp tiền",
    4: "Chi tiêu",
};

export const STATUS_MAPPING = {
    Draft: "Bản Nháp",
    Ongoing: "Đang Tiến Hành",
    Hiatus: "Tạm Dừng",
    Complete: "Hoàn Thành",
    Trashed: "Đã Xóa",
} as const;

export const STATUS_MAPPING_REVERSE = {
    "Bản Nháp": "Draft",
    "Đang Tiến Hành": "Ongoing",
    "Tạm Dừng": "Hiatus",
    "Hoàn Thành": "Complete",
    "Đã Xóa": "Trashed",
} as const;

export type APIRatingType = "All Ages" | "Teen" | "Young Adult" | "Mature";

export type VNRatingType =
    | "Tất cả độ tuổi"
    | "Thiếu niên"
    | "Thanh niên"
    | "Trưởng thành";

export const RATING_MAPPING = {
    "All Ages": "Tất cả độ tuổi",
    Teen: "Thiếu niên",
    "Young Adult": "Thanh niên",
    Mature: "Trưởng thành",
} as const;

export const RATING_MAPPING_REVERSE = {
    "Tất cả độ tuổi": "All Ages",
    "Thiếu niên": "Teen",
    "Thanh niên": "Young Adult",
    "Trưởng thành": "Mature",
} as const;

export const ROLES = {
    Administrator: "Administrator",
    Moderator: "Moderator",
};

export const CATEGORY_TRANSLATIONS: Record<string, string> = {
    sexual: "Nội dung người lớn",
    "sexual/minors": "Nội dung người lớn liên quan trẻ em",
    harassment: "Quấy rối",
    "harassment/threatening": "Quấy rối đe dọa",
    hate: "Phân biệt đối xử",
    "hate/threatening": "Đe dọa phân biệt đối xử",
    illicit: "Phi pháp",
    "illicit/violent": "Bạo lực phi pháp",
    "self-harm": "Tự hại",
    "self-harm/intent": "Ý định tự hại",
    "self-harm/instructions": "Hướng dẫn tự hại",
    violence: "Bạo lực",
    "violence/graphic": "Bạo lực đồ họa",
};
