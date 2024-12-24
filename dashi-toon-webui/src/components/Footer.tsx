import Link from "next/link";
import { Facebook, Twitter, Instagram, Book } from "lucide-react";
import Image from "next/image";
import UwUMgmt from "./UwUMgmt";

export default function Footer() {
    return (
        <footer className="border border-neutral-800 bg-neutral-900 text-neutral-300">
            <div className="container mx-auto px-4 py-8 sm:px-6 lg:px-8">
                <div className="grid grid-cols-1 gap-8 md:grid-cols-4">
                    <UwUMgmt>
                        <div>
                            <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-white">
                                <Image
                                    src="/logo.svg"
                                    width={32}
                                    height={32}
                                    alt="logo"
                                />
                                <span>DashiToon</span>
                            </h2>
                            <p className="text-sm">
                                Nền tảng đọc truyện trực tuyến hàng đầu Việt
                                Nam.
                            </p>
                        </div>
                    </UwUMgmt>
                    <div>
                        <h3 className="text-md mb-4 font-semibold text-white">
                            Khám phá
                        </h3>
                        <ul className="space-y-2">
                            <li>
                                <Link
                                    href="/browse"
                                    className="text-sm hover:text-blue-400"
                                >
                                    Duyệt truyện
                                </Link>
                            </li>
                            <li>
                                <Link
                                    href="/categories"
                                    className="text-sm hover:text-blue-400"
                                >
                                    Thể loại
                                </Link>
                            </li>
                            <li>
                                <Link
                                    href="/rankings"
                                    className="text-sm hover:text-blue-400"
                                >
                                    Bảng xếp hạng
                                </Link>
                            </li>
                            <li>
                                <Link
                                    href="/new-releases"
                                    className="text-sm hover:text-blue-400"
                                >
                                    Mới cập nhật
                                </Link>
                            </li>
                        </ul>
                    </div>
                    <div>
                        <h3 className="text-md mb-4 font-semibold text-white">
                            Hỗ trợ
                        </h3>
                        <ul className="space-y-2">
                            <li>
                                <Link
                                    href="/faq"
                                    className="text-sm hover:text-blue-400"
                                >
                                    FAQ
                                </Link>
                            </li>
                            <li>
                                <Link
                                    href="/contact"
                                    className="text-sm hover:text-blue-400"
                                >
                                    Liên hệ
                                </Link>
                            </li>
                            <li>
                                <Link
                                    href="/terms"
                                    className="text-sm hover:text-blue-400"
                                >
                                    Điều khoản sử dụng
                                </Link>
                            </li>
                            <li>
                                <Link
                                    href="/privacy"
                                    className="text-sm hover:text-blue-400"
                                >
                                    Chính sách bảo mật
                                </Link>
                            </li>
                        </ul>
                    </div>
                    <div>
                        <h3 className="text-md mb-4 font-semibold text-white">
                            Kết nối
                        </h3>
                        <div className="flex space-x-4">
                            <a
                                href="https://facebook.com"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-neutral-300 hover:text-blue-400"
                            >
                                <Facebook size={24} />
                                <span className="sr-only">Facebook</span>
                            </a>
                            <a
                                href="https://twitter.com"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-neutral-300 hover:text-blue-400"
                            >
                                <Twitter size={24} />
                                <span className="sr-only">Twitter</span>
                            </a>
                            <a
                                href="https://instagram.com"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-neutral-300 hover:text-blue-400"
                            >
                                <Instagram size={24} />
                                <span className="sr-only">Instagram</span>
                            </a>
                        </div>
                    </div>
                </div>
                <div className="mt-8 border-t border-neutral-800 pt-8 text-center text-sm">
                    <p>
                        &copy; {new Date().getFullYear()} DashiToon. Tất cả các
                        quyền được bảo lưu.
                    </p>
                </div>
            </div>
        </footer>
    );
}
