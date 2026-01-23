



  import { createRoot } from "react-dom/client";
  import App from "./app/App.tsx";
  import "./styles/index.css";

  // Version marker - if you don't see this in console, the build is outdated
  console.log('=== GM Buddy React v5 ===');

  createRoot(document.getElementById("root")!).render(<App />);
  
  
  
  