import Image from "next/image";

interface UserAvatarProps {
  user: { name: string; avatarUrl?: string };
}

export function UserAvatar({ user }: UserAvatarProps) {
  if (user.avatarUrl) {
    return (
      <Image
        src={user.avatarUrl}
        alt={`Avatar de ${user.name}`}
        width={40}
        height={40}
        unoptimized
        className="h-10 w-10 shrink-0 rounded-full object-cover"
      />
    );
  }

  const initial = user.name.trim().charAt(0).toUpperCase() || "U";

  return (
    <span aria-hidden="true" className="inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary font-heading text-[16px] font-bold text-white">
      {initial}
    </span>
  );
}
