import './splash.css';

export function Splash() {
  return (
    <div className="splash">
      <div className="splash-mark">Smena.Client</div>
      <div className="splash-drawer" aria-hidden="true">
        <div className="splash-drawer-slot" />
        <div className="splash-drawer-slot" />
        <div className="splash-drawer-slot" />
      </div>
      <div className="splash-status">Открываю кассу…</div>
    </div>
  );
}
