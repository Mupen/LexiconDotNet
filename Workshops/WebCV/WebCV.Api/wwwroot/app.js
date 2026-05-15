// ============================================================================
// App setup
// ============================================================================

const cvRoot = document.querySelector("#cv");
const desktopContentLayout = window.matchMedia("(min-width: 820px)");

loadCv();

// ============================================================================
// Data loading
// ============================================================================

async function loadCv() {
  try {
    const response = await fetch("/api/cv");

    if (!response.ok) {
      throw new Error(`The CV API returned ${response.status}.`);
    }

    const cv = await response.json();
    renderCv(cv);
  } catch (error) {
    renderError(error);
  }
}

// ============================================================================
// Page rendering
// ============================================================================

function renderCv(cv) {
  // Hobbies are personal context, so they render after the professional CV sections.
  const mainSections = cv.sections.filter(section => section.layout !== "interests");
  const footerSections = cv.sections.filter(section => section.layout === "interests");

  cvRoot.replaceChildren(
    createPortrait(cv),
    createMainContent(mainSections),
    createFooterContent(footerSections)
  );
}

// ============================================================================
// Portrait / profile header
// ============================================================================

function createPortrait(cv) {
  const portrait = element("header", "portrait");
  const profileMedia = createProfileMedia(cv.fullName);
  const identity = element("div", "identity");
  const contact = element("aside", "contact-panel");
  const about = element("section", "about-panel");

  identity.append(
    element("p", "eyebrow", "WebCV"),
    element("h1", null, cv.fullName),
    element("p", "role", cv.title)
  );

  about.append(
    element("h2", null, "About me"),
    element("p", "summary", cv.summary)
  );

  contact.append(
    createContactRow("Location", cv.location),
    createContactRow("Email", cv.email, `mailto:${cv.email}`),
    createContactRow("Phone", cv.phone, `tel:${cv.phone.replaceAll(" ", "")}`),
    createSocialLinks(cv.socialLinks)
  );

  portrait.append(profileMedia, identity, contact, about);
  return portrait;
}

function createProfileMedia(fullName) {
  const media = element("figure", "profile-media");
  const image = element("img", "profile-image");
  const fallback = element("figcaption", "profile-fallback", getInitials(fullName));

  image.src = "images/Profile_Image_5.png";
  image.alt = `${fullName} profile photo`;
  image.addEventListener("error", () => media.classList.add("missing-image"), { once: true });

  media.append(image, fallback);
  return media;
}

function getInitials(fullName) {
  return fullName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map(part => part[0])
    .join("")
    .toUpperCase();
}

// ============================================================================
// Main content and footer sections
// ============================================================================

function createMainContent(sections) {
  const content = element("div", "content-grid");
  const sectionElements = sections.map(createSection);

  arrangeSections(content, sectionElements);
  desktopContentLayout.addEventListener("change", () => arrangeSections(content, sectionElements));

  return content;
}

function createFooterContent(sections) {
  const footer = element("footer", "cv-footer");

  for (const section of sections) {
    footer.append(createSection(section));
  }

  return footer;
}

function arrangeSections(content, sectionElements) {
  if (!desktopContentLayout.matches) {
    content.replaceChildren(...sectionElements);
    return;
  }

  const leftColumn = element("div", "content-column");
  const rightColumn = element("div", "content-column");
  const splitIndex = Math.ceil(sectionElements.length / 2);

  // Natural-height columns avoid empty grid-row gaps when sections have different heights.
  sectionElements.forEach((sectionElement, index) => {
    const column = index < splitIndex ? leftColumn : rightColumn;
    column.append(sectionElement);
  });

  content.replaceChildren(leftColumn, rightColumn);
}

function createSection(section) {
  const sectionElement = element("section", `cv-section ${section.layout}`);
  sectionElement.append(element("h2", null, section.heading));

  const list = element("div", "item-list");

  for (const item of section.items) {
    list.append(createSectionItem(item, section.layout));
  }

  sectionElement.append(list);
  return sectionElement;
}

function createSectionItem(item, layout) {
  const article = element("article", `cv-item ${layout}`);
  const header = element("div", "item-header");

  header.append(
    element("h3", null, item.title),
    ...(layout === "interests" ? [] : [element("p", "period", item.period)])
  );

  article.append(
    header,
    element("p", "subtitle", item.subtitle),
    element("p", "description", item.description)
  );

  if (item.tags.length > 0 && layout !== "interests") {
    const tagList = element("ul", "tags");

    for (const tag of item.tags) {
      tagList.append(element("li", null, tag));
    }

    article.append(tagList);
  }

  return article;
}

// ============================================================================
// Contact details and social links
// ============================================================================

function createContactRow(label, value, href = null) {
  const row = element("p", "contact-row");
  row.append(element("span", null, label));

  if (href) {
    const link = element("a", null, value);
    link.href = href;
    row.append(link);
  } else {
    row.append(document.createTextNode(value));
  }

  return row;
}

function createSocialLinks(links) {
  const nav = element("nav", "social-links");
  nav.setAttribute("aria-label", "Social links");

  for (const socialLink of links) {
    const link = element("a", null, socialLink.label);
    link.href = socialLink.url;
    link.rel = "noreferrer";
    link.target = "_blank";
    nav.append(link);
  }

  return nav;
}

// ============================================================================
// Error state
// ============================================================================

function renderError(error) {
  cvRoot.replaceChildren();
  const errorSection = element("section", "error");
  errorSection.append(
    element("h1", null, "CV could not be loaded"),
    element("p", null, error.message)
  );
  cvRoot.append(errorSection);
}

// ============================================================================
// DOM helper
// ============================================================================

function element(tagName, className = null, text = null) {
  const node = document.createElement(tagName);

  if (className) {
    node.className = className;
  }

  if (text !== null && text !== undefined) {
    node.textContent = text;
  }

  return node;
}
