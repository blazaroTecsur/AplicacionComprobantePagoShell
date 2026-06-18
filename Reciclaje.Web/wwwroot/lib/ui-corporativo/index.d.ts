// @corp/design-system — TypeScript Definitions v1.0.0
// Auto-generated — do not edit manually

export interface ComboOptions {
  placeholder?:       string;
  searchPlaceholder?: string;
  noResultsText?:     string;
  searchable?:        boolean;
  multiple?:          boolean;
  onChange?:          (value: string | string[], item?: any) => void;
}

export interface ComboInstance {
  getValue():  string | string[];
  setValue(val: string): void;
  setValues(vals: string[]): void;
  destroy(): void;
}

export interface TableOptions {
  sortable?:   boolean;
  searchable?: boolean;
  pageSize?:   number;
  pageSizes?:  number[];
  selectable?: boolean;
}

export interface TableInstance {
  refresh(): void;
  goTo(page: number): void;
  getSelected(): string[];
  setLoading(val: boolean): void;
}

export interface ModalOptions {
  title?:       string;
  size?:        '' | 'sm' | 'lg' | 'xl';
  variant?:     '' | 'danger' | 'success' | 'warning';
  body?:        string;
  footer?:      string;
  onConfirm?:   () => void;
  onClose?:     () => void;
}

export interface ConfirmOptions {
  title?:       string;
  message?:     string;
  variant?:     'danger' | 'warning' | 'info';
  confirmText?: string;
}

export interface ToastOptions {
  type?:     'success' | 'danger' | 'warning' | 'info';
  title?:    string;
  message?:  string;
  duration?: number;
  closable?: boolean;
}

export declare const Button: {
  setLoading(btn: HTMLElement | string, text?: string): void;
  stopLoading(btn: HTMLElement | string): void;
};

export declare const Combo: {
  new(element: HTMLElement | string, options?: ComboOptions): ComboInstance;
};

export declare const Table: {
  new(element: HTMLElement | string, options?: TableOptions): TableInstance;
};

export declare const Modal: {
  open(idOrEl: HTMLElement | string | null, options?: ModalOptions): HTMLElement;
  close(idOrEl?: HTMLElement | string): void;
  confirm(options: ConfirmOptions): Promise<boolean>;
};

export declare const Toast: {
  show(options: ToastOptions | string): HTMLElement;
  success(message: string, title?: string): HTMLElement;
  danger(message: string, title?: string): HTMLElement;
  warning(message: string, title?: string): HTMLElement;
  info(message: string, title?: string): HTMLElement;
};

export declare const Alert: {
  init(container?: string): void;
};

export declare const Loading: {
  show(message?: string): void;
  hide(): void;
  wrap<T>(fn: () => Promise<T>, message?: string): Promise<T>;
};

declare const CorporateDS: {
  Button:  typeof Button;
  Combo:   typeof Combo;
  Table:   typeof Table;
  Modal:   typeof Modal;
  Toast:   typeof Toast;
  Alert:   typeof Alert;
  Loading: typeof Loading;
};

export default CorporateDS;
